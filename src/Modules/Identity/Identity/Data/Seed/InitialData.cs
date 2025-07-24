using Module = Identity.Modules.Models.Module;

namespace Identity.Data.Seed;

public static class InitialData
{
    public static IEnumerable<PermissionType> TiposPermisos =>
        [
            PermissionType.Create("Crear", "VIEW", "Permite crear nuevos registros"),
            PermissionType.Create("Leer", "ADD", "Permite visualizar registros"),
            PermissionType.Create("Actualizar", "EDIT", "Permite modificar registros existentes"),
            PermissionType.Create("Eliminar", "REMOVE", "Permite eliminar registros"),
            PermissionType.Create("Administrar", "ADMIN", "Acceso completo al módulo"),
        ];

    public static IEnumerable<Module> Modules =>
        [
            Module.Create("Catalog", "Gestión de productos y categorías"),
            Module.Create("Basket", "Gestión de carritos de compra"),
            Module.Create("Ordering", "Gestión de órdenes y pedidos"),
            Module.Create("Identity", "Gestión de usuarios y permisos"),
        ];

    public static IEnumerable<Role> Roles =>
        [
            Role.Create("Administrador", "Acceso completo al sistema"),
            Role.Create("Usuario", "Acceso básico al sistema"),
            Role.Create("Operador", "Acceso para operaciones específicas"),
        ];
}
