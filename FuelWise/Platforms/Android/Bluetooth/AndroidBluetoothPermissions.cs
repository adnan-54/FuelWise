using static Microsoft.Maui.ApplicationModel.Permissions;

namespace FuelWise.Platforms.Android;

internal class AndroidBluetoothPermissions : BasePlatformPermission
{
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
        [
            ("android.permission.BLUETOOTH", true),
            ("android.permission.BLUETOOTH_ADMIN", true),
            ("android.permission.BLUETOOTH_SCAN", true),
            ("android.permission.BLUETOOTH_CONNECT", true),
            ("android.permission.BLUETOOTH_ADVERTISE", true),
            ("android.permission.ACCESS_COARSE_LOCATION", true),
            ("android.permission.ACCESS_FINE_LOCATION", true)
        ];
}