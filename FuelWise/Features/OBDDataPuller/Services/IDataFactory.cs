using FuelWise.OBDProtocol;
using Microsoft.Maui.Controls;
using Frame = FuelWise.OBDProtocol.Frame;

namespace FuelWise.OBDDataPuller;

public interface IDataFactory
{
    PID GetPID<TType>() where TType : class, IOBDData;

    TType CreateResponse<TType>(Frame frame) where TType : class, IOBDData;

    TType CreateRequest<TType>() where TType : class, IOBDData;
}

internal class DefaultDataFactory : IDataFactory
{
    private readonly Dictionary<Type, PID> pidCache;
    private readonly Dictionary<PID, Type> dataCache;

    public DefaultDataFactory()
    {
        pidCache = [];
        dataCache = [];
    }

    PID IDataFactory.GetPID<TType>()
    {
        EnsurePidCache();

        if (pidCache.TryGetValue(typeof(TType), out var pid))
            return pid;

        return default;
    }

    TType IDataFactory.CreateResponse<TType>(Frame frame)
    {
        EnsurePidCache();

        var pid = frame.Payload.PID;

        if (!dataCache.TryGetValue(pid, out var dataType))
            throw new Exception($"Data type for PID {pid} not found");

        var instance = Activator.CreateInstance(dataType, frame);

        if (instance is not TType data)
            throw new Exception($"Data type {dataType.Name} does not implement {typeof(TType).Name}");

        return data;
    }

    TType IDataFactory.CreateRequest<TType>()
    {
        EnsurePidCache();

        var pid = ((IDataFactory)this).GetPID<TType>();

        var data = new Data();
        var payload = new Payload(Mode.ShowCurrentData, pid, data);
        var frame = new Frame(CanId.Request, payload);

        if (Activator.CreateInstance(typeof(TType), frame) is not TType request)
            throw new Exception($"Request type {typeof(TType).Name} does not implement {typeof(IOBDData).Name}");

        return request;
    }

    private void EnsurePidCache()
    {
        if (pidCache.Any())
            return;

        var dataTypes = GetType().Assembly.GetTypes().Where(t => typeof(IOBDData).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
        var enumNames = Enum.GetNames<PID>();

        foreach (var dataType in dataTypes)
        {
            var typeName = dataType.Name.Replace("data", string.Empty, StringComparison.CurrentCultureIgnoreCase);
            var enumName = enumNames.FirstOrDefault(n => n.Contains(typeName, StringComparison.CurrentCultureIgnoreCase));

            if (string.IsNullOrEmpty(enumName))
                throw new Exception($"PID for {dataType.Name} not found");

            var pid = Enum.Parse<PID>(enumName);

            if (pidCache.ContainsValue(pid))
                throw new Exception($"PID {pid:X2} already exists");

            pidCache.Add(dataType, pid);
            dataCache.Add(pid, dataType);
        }
    }
}