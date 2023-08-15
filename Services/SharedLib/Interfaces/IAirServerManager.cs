using ucip_lib_v5.models;

namespace SharedLib.Interfaces
{
    public interface IAirServerManager
    {
        AirServer GetDifferentActiveAir(AirServer airServer);
        AirServer GetRandomActiveAir();
    }
}