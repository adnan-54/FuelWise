using System.Collections.Concurrent;
using FuelWise.OBDProtocol;
using Frame = FuelWise.OBDProtocol.Frame;

namespace FuelWise.OBDDataPuller;

public interface IDataPuller
{
    void RequestData<TType>() where TType : class, IOBDData;

    Task<TType> PullDataAsync<TType>(CancellationToken cancellationToken = default) where TType : class, IOBDData;

    void SubscribeToData<TType>(Action<TType> callback) where TType : class, IOBDData;

    void UnsubscribeFromData<TType>(Action<TType> callback) where TType : class, IOBDData;
}

internal class DefaultDataPuller : IDataPuller
{
    private static readonly TimeSpan REQUEST_TIMEOUT = TimeSpan.FromSeconds(2);

    private readonly IRequestProcessor requestProcessor;
    private readonly IDataFactory dataFactory;

    private readonly ConcurrentDictionary<Type, IList<(Action, Action<Frame>)>> requestedDataTypes;
    private readonly IList<IOBDData> receivedData;
    private readonly IList<Action<IOBDData>> dataReceivedHandlers;

    public DefaultDataPuller(IRequestProcessor requestProcessor, IDataFactory dataFactory)
    {
        this.requestProcessor = requestProcessor;
        this.dataFactory = dataFactory;
        requestProcessor.FrameReceived += OnFrameReceived;

        requestedDataTypes = [];
        receivedData = [];
        dataReceivedHandlers = [];
    }

    void IDataPuller.RequestData<TType>()
    {
        var request = dataFactory.CreateRequest<TType>();
        requestProcessor.QueueRequest(request);
    }

    Task<TType> IDataPuller.PullDataAsync<TType>(CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<TType>();
        var pid = dataFactory.GetPID<TType>();

        var request = dataFactory.CreateRequest<TType>();
        var requestType = request.GetType();

        if (!requestedDataTypes.ContainsKey(requestType))
            requestedDataTypes.TryAdd(requestType, []);

        var timeoutCts = new CancellationTokenSource(REQUEST_TIMEOUT);
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
        cts.Token.Register(EndTaskCallback, false);

        requestedDataTypes[requestType].Add((EndTaskCallback, ResponseCallback));

        requestProcessor.QueueRequest(request);

        return tcs.Task;

        void EndTaskCallback()
        {
            if (tcs.Task.IsCompleted)
                return;

            if (timeoutCts.IsCancellationRequested)
                tcs.TrySetException(new TimeoutException());
            else
            if (cancellationToken.IsCancellationRequested)
                tcs.TrySetCanceled(cancellationToken);
            else
                tcs.TrySetException(new NotSupportedException());

            timeoutCts.Dispose();

            if (requestedDataTypes.TryGetValue(requestType, out var callbacks))
                callbacks.Remove((EndTaskCallback, ResponseCallback));
        }

        void ResponseCallback(Frame frame)
        {
            try
            {
                var response = dataFactory.CreateResponse<TType>(frame);
                tcs.SetResult(response);
                QueueDataReceived(response);
            }
            catch (Exception e)
            {
                tcs.TrySetException(e);
            }
            finally
            {
                timeoutCts.Dispose();
            }
        }
    }

    void IDataPuller.SubscribeToData<TType>(Action<TType> callback)
    {
        var type = typeof(TType);

        dataReceivedHandlers.Add((data) =>
        {
            if (data is TType receivedData)
                callback(receivedData);
        });
    }

    void IDataPuller.UnsubscribeFromData<TType>(Action<TType> callback)
    {
        var type = typeof(TType);

        dataReceivedHandlers.Remove((data) =>
        {
            if (data is TType receivedData)
                callback(receivedData);
        });
    }

    private void OnFrameReceived(object sender, FrameReceivedEventArgs e)
    {
        var frame = e.Data;
        var requestType = e.Request.GetType();
        var hasFaulted = frame.Payload.Mode == Mode.Error;

        if (requestedDataTypes.TryGetValue(requestType, out var callbacks))
        {
            foreach (var (reject, accept) in callbacks.ToList())
            {
                if (hasFaulted)
                    reject();
                else
                    accept(frame);
            }

            callbacks.Clear();
        }

        RaiseDataReceived();
    }

    private void QueueDataReceived<TType>(TType data) where TType : class, IOBDData
    {
        if (receivedData.Contains(data))
            return;

        receivedData.Add(data);
    }

    private void RaiseDataReceived()
    {
        foreach (var data in receivedData)
        {
            foreach (var handler in dataReceivedHandlers)
                handler(data);
        }

        receivedData.Clear();
    }
}