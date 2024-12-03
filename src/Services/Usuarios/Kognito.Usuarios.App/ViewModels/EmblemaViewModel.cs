using Kognito.Usuarios.App.Domain;

namespace Kognito.Usuarios.App.ViewModels;

public class EmblemaViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime? UnlockedOn { get; set; }

    public static EmblemaViewModel Mapear(Emblemas emblema)
    {
        if (emblema == null) return null;

        return new EmblemaViewModel
        {
            Id = emblema.Id,
            Name = emblema.Nome,
            Description = emblema.Descricao,
            UnlockedOn = emblema.DesbloqueadoEm
        };
    }
}