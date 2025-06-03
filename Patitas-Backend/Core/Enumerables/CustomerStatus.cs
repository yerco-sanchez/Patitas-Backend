namespace Patitas_Backend.Core.Enumerables;
public enum CustomerStatus
{
    // El cliente está activo y en contacto frecuente con la veterinaria
    Active = 1,

    // El cliente no ha interactuado con la clínica por un largo período
    Inactive = 2,

    // Deuda pendiente que ha excedido el plazo de pago
    Overdue = 3,

    // Cliente con deuda pendiente
    InDebt = 4,

    // Cliente con un estatus preferencial por su lealtad
    VIP = 5,
}
