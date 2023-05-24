using static Microsoft.Maui.ApplicationModel.Permissions;

namespace TG2;

internal class BluetoothPermissions : BasePlatformPermission
{
#if ANDROID
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions => new List<(string permissions, bool isRuntime)>
    {
        ("android.permission.BLUETOOTH", true),
        ("android.permission.BLUETOOTH_ADMIN" , true),
        ("android.permission.BLUETOOTH_SCAN" , true),
        ("android.permission.BLUETOOTH_CONNECT" , true),
        ("android.permission.BLUETOOTH_ADVERTISE" , true),
    }.ToArray();
#endif
}
