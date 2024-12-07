using EstartandoDevsCore.Mediator;
using EstartandoDevsWebApiCore.Controllers;
using Kognito.Turmas.App.Commands;
using Kognito.Turmas.App.Queries;
using Kognito.Turmas.Domain;
using Kognito.WebApi.InputModels;
using Microsoft.AspNetCore.Mvc;
using static Enturmamento;


namespace Kognito.WebApi.Controllers;

[Route("api/turmas")]
public class TurmasController : MainController
{
    private readonly ITurmaQueries _turmaQueries;
    private readonly IMediatorHandler _mediatorHandler;

    public TurmasController(ITurmaQueries turmaQueries, IMediatorHandler mediatorHandler)
    {
        _turmaQueries = turmaQueries;
        _mediatorHandler = mediatorHandler;
    }
    

   private async Task<Guid?> ObterUsuarioIdPorIdentityId()
{
    #if DEBUG
        return new Guid("11111111-1111-1111-1111-111111111111");
    #else
        var identityId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(identityId)) return null;

        var usuario = await _usuarioQueries.ObterPorEmail(User.FindFirst(ClaimTypes.Email)?.Value);
        return usuario?.Id;
    #endif
}
    
    /// <summary>
    /// Obtém todas as turmas de um professor específico
    /// </summary>
    /// <param name="professorId">ID do professor para consulta</param>
    /// <returns>Lista de turmas associadas ao professor</returns>
    /// <response code="200">Retorna a lista de turmas do professor</response>
    /// <response code="400">Quando o ID do professor é inválido</response>
    /// <response code="404">Quando o professor não é encontrado</response>
    /// <response code="500">Quando ocorre um erro interno ao buscar as turmas</response>

    [HttpGet("professor/{professorId}")]
    public async Task<IActionResult> ObterTurmasProfessor(Guid professorId)
    {
        if (professorId == Guid.Empty)
        {
            AdicionarErro("Id do professor inválido");
            return BadRequest();
        }
        var turmas = await _turmaQueries.ObterTurmasPorProfessor(professorId);
        return CustomResponse(turmas);
    }

    /// <summary>
    /// Obtém todas as turmas em que um aluno está matriculado
    /// </summary>
    /// <param name="alunoId">ID do aluno para consulta</param>
    /// <returns>Lista de turmas em que o aluno está matriculado</returns>
    /// <response code="200">Retorna a lista de turmas do aluno</response>
    /// <response code="400">Quando o ID do aluno é inválido</response>
    /// <response code="404">Quando o aluno não é encontrado</response>
    /// <response code="500">Quando ocorre um erro interno ao buscar as turmas</response>
    [HttpGet("aluno/{alunoId}")]
    public async Task<IActionResult> ObterTurmasAluno(Guid alunoId)
    {
        if (alunoId == Guid.Empty)
        {
            AdicionarErro("Id do aluno inválido");
            return BadRequest();
        }
        var turmas = await _turmaQueries.ObterTurmasPorAluno(alunoId);
        return CustomResponse(turmas);
}


    /// <summary>
    /// Obtém os detalhes completos de uma turma específica
    /// </summary>
    /// <param name="id">ID da turma para consulta</param>
    /// <returns>Dados detalhados da turma</returns>
    /// <response code="200">Retorna os dados completos da turma</response>
    /// <response code="404">Quando a turma não é encontrada</response>
    /// <response code="500">Quando ocorre um erro interno ao buscar a turma</response>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var turma = await _turmaQueries.ObterPorId(id);
        if (turma == null)
        {
            AdicionarErro("Turma não encontrada");
            return NotFound();
        }
        return CustomResponse(turma);
    }



    /// <summary>
    /// Obtém a quantidade total de alunos matriculados em uma turma específica
    /// </summary>
    /// <param name="turmaId">ID da turma para consulta</param>
    /// <returns>Número total de alunos na turma</returns>
    /// <response code="200">Retorna a quantidade de alunos na turma</response>
    /// <response code="400">Quando o ID da turma é inválido</response>
    /// <response code="404">Quando a turma não é encontrada</response>
    /// <response code="500">Quando ocorre um erro interno ao obter a quantidade de alunos</response>
    [HttpGet("{turmaId:guid}/alunos/quantidade")]
    public async Task<IActionResult> ObterQuantidadeAlunos(Guid turmaId)
    {
        var quantidade = await _turmaQueries.ObterQuantidadeAlunos(turmaId);
        return CustomResponse(new { QuantidadeAlunos = quantidade });
    }


    /// <summary>
    /// Obtém o hash de acesso único que permite que alunos ingressem na turma
    /// </summary>
    /// <param name="turmaId">ID da turma</param>
    /// <returns>Hash de acesso da turma</returns>
    /// <response code="200">Retorna o hash de acesso da turma</response>
    /// <response code="400">Quando o ID da turma é inválido</response>
    /// <response code="404">Quando a turma não é encontrada</response>
    /// <response code="500">Quando ocorre um erro interno ao obter o hash de acesso</response>
    [HttpGet("{turmaId:guid}/acesso")]
    public async Task<IActionResult> ObterAcessoTurma(Guid turmaId)
    {
        var acesso = await _turmaQueries.ObterAcessoTurma(turmaId);
        return CustomResponse(acesso);
    }


    /// <summary>
    /// Cria uma nova turma com um professor responsável
    /// </summary>
    /// <param name="model">Dados da turma contendo nome, descrição, matéria, cor e ícone</param>
    /// <returns>Resultado da criação da turma</returns>
    /// <response code="200">Turma criada com sucesso</response>
    /// <response code="400">Quando os dados da requisição são inválidos</response>
    /// <response code="401">Quando o usuário não está autenticado</response>
    /// <response code="500">Quando ocorre um erro interno ao criar a turma</response>
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] TurmaInputModel model)
    {
        if (!ModelState.IsValid) return CustomResponse(ModelState);

        var usuarioId = await ObterUsuarioIdPorIdentityId();
        if (!usuarioId.HasValue) return Unauthorized();

        var command = new CriarTurmaCommand(
            professor: new Usuario("Professor", usuarioId.Value),
            nome: model.Name,
            descricao: model.Description,
            materia: model.Subject,
            cor: model.Color,
            icone: model.Icon
        );

        var result = await _mediatorHandler.EnviarComando(command);
        if (result.IsValid)
            return CustomResponse("Turma criada com sucesso!");
        return CustomResponse(result);
    }

    /// <summary>
    /// Permite que um aluno ingresse em uma turma existente usando o hash de acesso único
    /// </summary>
    /// <param name="hashAcesso">Hash único de acesso da turma</param>
    /// <param name="model">Dados do aluno contendo ID e nome</param>
    /// <returns>Confirmação do ingresso e dados da turma</returns>
    /// <response code="200">Ingresso realizado com sucesso, retornando dados da turma</response>
    /// <response code="400">Quando os dados são inválidos ou o professor tenta ingressar como aluno</response>
    /// <response code="404">Quando a turma não é encontrada pelo hash de acesso</response>
    /// <response code="500">Quando ocorre um erro interno ao realizar o ingresso</response>
    [HttpPost("ingressar/{hashAcesso}")]
        public async Task<IActionResult> IngressarTurma(string hashAcesso, [FromBody] IngressoTurmaInputModel model)
        {
        var turma = await _turmaQueries.ObterPorHashAcesso(hashAcesso);
        if (turma == null)
        {
            AdicionarErro("Turma não encontrada");
            return CustomResponse();
        }
        
        if (string.IsNullOrWhiteSpace(hashAcesso))
        {
            AdicionarErro("Hash de acesso inválido");
            return BadRequest();
        }

        if (turma.Professor.Id == model.StudentId)
        {
            AdicionarErro("Professor não pode ingressar como aluno em sua própria turma");
            return CustomResponse();
        }

        var command = new CriarAlunoNaTurmaCommand(
            id: Guid.NewGuid(),
            alunoId: model.StudentId,
            alunoNome: model.StudentName,
            turmaId: turma.Id,
            status: Enturmamento.EnturtamentoStatus.Ativo
        );

        var result = await _mediatorHandler.EnviarComando(command);
        
        if (result.IsValid)
            return CustomResponse(new 
            { 
                mensagem = "Ingresso na turma realizado com sucesso!",
                turma = turma
            });
            
        return CustomResponse(result);
    }

    /// <summary>
    /// Atualiza as informações de uma turma existente
    /// </summary>
    /// <param name="id">ID da turma a ser atualizada</param>
    /// <param name="model">Novos dados da turma</param>
    /// <returns>Dados atualizados da turma</returns>
    /// <response code="200">Retorna a turma com os dados atualizados</response>
    /// <response code="400">Quando os dados são inválidos</response>
    /// <response code="401">Quando o usuário não está autenticado</response>
    /// <response code="404">Quando a turma não é encontrada</response>
    /// <response code="500">Quando ocorre um erro interno ao atualizar a turma</response>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] TurmaInputModel model)
    {
        if (!ModelState.IsValid) return CustomResponse(ModelState);

        var usuarioId = await ObterUsuarioIdPorIdentityId();
        if (!usuarioId.HasValue) return Unauthorized();

        var command = new AtualizarTurmaCommand(
            id: id,
            professor: new Usuario("Professor", usuarioId.Value),
            nome: model.Name,
            descricao: model.Description,
            materia: model.Subject
        );

        var result = await _mediatorHandler.EnviarComando(command);
        if (result.IsValid)
        {
            var turmaAtualizada = await _turmaQueries.ObterPorId(id);
            return CustomResponse(turmaAtualizada);
        }
        
        return CustomResponse(result);
    }

    /// <summary>
    /// Remove permanentemente uma turma e todos seus relacionamentos
    /// </summary>
    /// <param name="id">ID da turma a ser removida</param>
    /// <returns>Confirmação da exclusão</returns>
    /// <response code="200">Turma excluída com sucesso</response>
    /// <response code="404">Quando a turma não é encontrada</response>
    /// <response code="500">Quando ocorre um erro interno ao excluir a turma</response>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Excluir(Guid id)
    {
        var command = new ExcluirTurmaCommand(id);
        var result = await _mediatorHandler.EnviarComando(command);
        
        if (result.IsValid)
            return CustomResponse("Turma excluída com sucesso!");
            
        return CustomResponse(result);
    }

    /// <summary>
    /// Vincula um conteúdo didático existente a uma turma específica
    /// </summary>
    /// <param name="turmaId">ID da turma</param>
    /// <param name="conteudoId">ID do conteúdo a ser vinculado</param>
    /// <returns>Confirmação do vínculo</returns>
    /// <response code="200">Conteúdo vinculado com sucesso à turma</response>
    /// <response code="400">Quando não foi possível vincular o conteúdo ou IDs inválidos</response>
    /// <response code="404">Quando a turma ou conteúdo não são encontrados</response>
    /// <response code="500">Quando ocorre um erro interno ao vincular</response>
    [HttpPost("{turmaId:guid}/vincular-conteudo/{conteudoId:guid}")]
    public async Task<IActionResult> VincularConteudo(Guid turmaId, Guid conteudoId)
    {
        try
        {
            if (turmaId == Guid.Empty || conteudoId == Guid.Empty)
            {
                AdicionarErro("Ids inválidos");
                return BadRequest();
            }

            var vinculado = await _turmaQueries.VincularTurma(conteudoId, turmaId);
            if (vinculado)
                return CustomResponse("Conteúdo vinculado com sucesso!");
                
            AdicionarErro("Não foi possível vincular o conteúdo à turma");
            return CustomResponse();
        }
        catch (Exception ex)
        {
            AdicionarErro($"Erro ao vincular conteúdo: {ex.Message}");
            return CustomResponse();
        }
    }


    /// <summary>
    /// Remove um aluno de uma turma específica, alterando seu status para inativo
    /// </summary>
    /// <param name="turmaId">ID da turma</param>
    /// <param name="alunoId">ID do aluno a ser removido</param>
    /// <returns>Confirmação da remoção do aluno</returns>
    /// <response code="200">Aluno removido com sucesso da turma</response>
    /// <response code="400">Quando ocorre um erro ao remover o aluno ou IDs inválidos</response>
    /// <response code="404">Quando a turma ou aluno não são encontrados</response>
    /// <response code="500">Quando ocorre um erro interno ao remover o aluno</response>
    [HttpDelete("{turmaId:guid}/alunos/{alunoId:guid}")]
    public async Task<IActionResult> ExcluirAluno(Guid turmaId, Guid alunoId)
    {
        try
        {
            var command = new ExcluirAlunoDaTurmaCommand(
                alunoId: alunoId,
                turmaId: turmaId,
                status: EnturtamentoStatus.Inativo
            );

            var result = await _mediatorHandler.EnviarComando(command);
            if (result.IsValid)
                return CustomResponse("Aluno removido da turma com sucesso!");
                
            return CustomResponse(result);
        }
        catch (Exception ex)
        {
            AdicionarErro($"Erro ao excluir aluno da turma: {ex.Message}");
            return CustomResponse();
        }
    }
}