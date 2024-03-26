using System.Globalization;
using System.Text;
using FuelWise.OBDEncoder;
using FuelWise.OBDProtocol;
using Frame = FuelWise.OBDProtocol.Frame;

namespace FuelWise.Tests;

[TestClass]
public class DefaultOBDEncoder_Should
{
    private readonly DefaultOBDEncoder sut;

    public DefaultOBDEncoder_Should()
    {
        sut = new DefaultOBDEncoder();
    }

    [TestMethod]
    public void Encode_WhenEncodingAny_ShouldReturn11Bytes()
    {
        //arrange
        var data = new Data();
        var payload = new Payload(Mode.ShowCurrentData, PID.ThrottlePosition, data);
        var frame = new Frame(CanId.Request, payload);

        //act
        var encoded = sut.Encode(frame);
        var asString = Encoding.ASCII.GetString(encoded);
        asString = $"0{asString}";
        var bytes = asString.Chunk(2)
            .Select(c => new string(c))
            .ToArray();

        //assert
        Assert.AreEqual(11, bytes.Length);
    }

    [TestMethod]
    public void Decode_WhenDecodingRequest_ShouldReturnIsRequest()
    {
        //arrange
        var buffer = Encoding.ASCII.GetBytes("7DF020111AAAAAAAAAA\r");

        //act
        var result = sut.Decode(buffer);

        //assert
        Assert.AreEqual(true, result.IsRequest);
    }
}