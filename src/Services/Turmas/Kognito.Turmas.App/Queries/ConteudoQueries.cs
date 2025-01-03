using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kognito.Turmas.App.ViewModels;
using Kognito.Turmas.Domain.Interfaces;

namespace Kognito.Turmas.App.Queries
{
    public class ConteudoQueries : IConteudoQueries
{
    private readonly IConteudoRepository _conteudoRepository;

    public ConteudoQueries(IConteudoRepository conteudoRepository)
    {
        _conteudoRepository = conteudoRepository;
    }

    public async Task<IEnumerable<ConteudoViewModel>> ObterPorId(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id do conteúdo inválido", nameof(id));

        var conteudo = await _conteudoRepository.ObterPorId(id);
        return conteudo == null 
            ? Enumerable.Empty<ConteudoViewModel>() 
            : new List<ConteudoViewModel> { ConteudoViewModel.Mapear(conteudo) };
    }

    public async Task<IEnumerable<ConteudoViewModel>> ObterTodosConteudos()
    {
        var conteudos = await _conteudoRepository.ObterTodosConteudo();
        return conteudos?.Select(ConteudoViewModel.Mapear) 
            ?? Enumerable.Empty<ConteudoViewModel>();
    }

    public async Task<IEnumerable<ConteudoViewModel>> ObterPorTurma(Guid turmaId)
    {
        if (turmaId == Guid.Empty)
            throw new ArgumentException("Id da turma inválido", nameof(turmaId));

        var conteudos = await _conteudoRepository.ObterPorTurma(turmaId);
        return conteudos?.Select(ConteudoViewModel.Mapear) 
            ?? Enumerable.Empty<ConteudoViewModel>();
    }

    public async Task<int> ObterQuantidadeConteudosPorTurma(Guid turmaId)
    {
        if (turmaId == Guid.Empty)
            throw new ArgumentException("Id da turma inválido", nameof(turmaId));

        return await _conteudoRepository.ObterQuantidadeConteudosPorTurma(turmaId);
    }
}
    
}