using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelWise.OBDProtocol;

public record PID(byte Code);

public record Mode(byte Code);

public record CanId(byte Id);
