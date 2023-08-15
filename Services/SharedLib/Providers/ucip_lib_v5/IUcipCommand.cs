using ucip_lib_v5.models;

namespace ucip_lib_v5
{
    public interface IUcipCommand
    {
        UcipCommandBuilder CreateBuilder(AirServer airServer);
    }
}