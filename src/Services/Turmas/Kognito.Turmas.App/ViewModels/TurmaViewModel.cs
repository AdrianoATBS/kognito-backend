using System;
using Kognito.Turmas.Domain;

namespace Kognito.Turmas.App.ViewModels;

public class TurmaViewModel
{ 
    public Guid Id { get; set; }
    public ProfessorViewModel Professor { get; set; }
    public string Nome { get; set; }
    public string Descricao { get; set; }
    public string Materia { get; set; }
    public string LinkAcesso { get; set; }
    public string Cor { get; set; }
    public static TurmaViewModel Mapear(Turma turma)
    {
        return new TurmaViewModel
        {
            Id = turma.Id,
            Professor = turma.Professor != null ? new ProfessorViewModel(turma.Professor.Id, turma.Professor.Nome) : null,
            Nome = turma.Nome,
            Descricao = turma.Descricao,
            Materia = turma.Materia,
            LinkAcesso = turma.LinkAcesso,
            Cor = turma.Cor,
        };
    }
}
