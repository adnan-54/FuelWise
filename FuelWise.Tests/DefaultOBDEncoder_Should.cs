using System.Text;
using FuelWise.OBDEncoder;
using FuelWise.OBDProtocol;
using Frame = FuelWise.OBDProtocol.Frame;

namespace FuelWise.Tests;

[TestClass]
public class DefaultOBDEncoder_Should
{
    private readonly DefaultOBDEncoder encoder;

    public DefaultOBDEncoder_Should()
    {
        encoder = new DefaultOBDEncoder();
    }

    [TestMethod]
    public void Encode_CorrectSize()
    {
        //arrange
        var data = new Data();
        var payload = new Payload(Mode.ShowCurrentData, PID.ThrottlePosition, data);
        var request = new Frame(CanId.Request, payload);

        //act
        var result = encoder.Encode(request);

        //assert
        Assert.AreEqual(18, result.Length);
    }

    [TestMethod]
    public void Decode_IsRequest()
    {
        //arrange
        var buffer = Encoding.ASCII.GetBytes("7DF020111AAAAAAAAAA\r");

        //act
        var result = encoder.Decode(buffer);

        //assert
        Assert.AreEqual(result.IsRequest, true);
    }
}