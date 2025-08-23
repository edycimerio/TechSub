using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechSub.Aplicacao.DTOs;
using TechSub.Dominio.Entidades;
using TechSub.Dominio.Interfaces;

namespace TechSub.WebAPI.Controllers;

/// <summary>
/// Controller para gerenciamento de usuários
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioRepository _usuarioRepository;

    public UsuariosController(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    /// <summary>
    /// Obtém todos os usuários ativos (apenas admins)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ObterTodos()
    {
        try
        {
            var usuarios = await _usuarioRepository.ObterTodosAtivosAsync();
            
            var usuariosDto = usuarios.Select(u => new UsuarioDto
            {
                Id = u.Id,
                Nome = u.Nome,
                Email = u.Email,
                AvatarUrl = u.AvatarUrl,
                Provedor = u.Provedor,
                DataUltimoLogin = u.DataUltimoLogin
            });

            return Ok(usuariosDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno no servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtém um usuário por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        try
        {
            var usuarioAtual = ObterUsuarioAtual();
            
            // Usuário só pode ver seus próprios dados, exceto admins
            if (usuarioAtual.Id != id && !User.IsInRole("Admin"))
            {
                return Forbid("Você só pode acessar seus próprios dados");
            }

            var usuario = await _usuarioRepository.ObterPorIdAsync(id);
            
            if (usuario == null)
            {
                return NotFound(new { message = "Usuário não encontrado" });
            }

            var usuarioDto = new UsuarioDto
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                AvatarUrl = usuario.AvatarUrl,
                Provedor = usuario.Provedor,
                DataUltimoLogin = usuario.DataUltimoLogin
            };

            return Ok(usuarioDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno no servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza dados do usuário
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarUsuarioDto dto)
    {
        try
        {
            var usuarioAtual = ObterUsuarioAtual();
            
            // Usuário só pode atualizar seus próprios dados, exceto admins
            if (usuarioAtual.Id != id && !User.IsInRole("Admin"))
            {
                return Forbid("Você só pode atualizar seus próprios dados");
            }

            var usuario = await _usuarioRepository.ObterPorIdAsync(id);
            
            if (usuario == null)
            {
                return NotFound(new { message = "Usuário não encontrado" });
            }

            // Verifica se email já existe (se foi alterado)
            if (dto.Email != usuario.Email)
            {
                var emailExiste = await _usuarioRepository.EmailExisteAsync(dto.Email);
                if (emailExiste)
                {
                    return BadRequest(new { message = "Email já está em uso" });
                }
            }

            // Atualiza dados
            usuario.Nome = dto.Nome;
            usuario.Email = dto.Email;
            usuario.AvatarUrl = dto.AvatarUrl;
            usuario.DataAtualizacao = DateTime.UtcNow;

            await _usuarioRepository.AtualizarAsync(usuario);

            var usuarioDto = new UsuarioDto
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                AvatarUrl = usuario.AvatarUrl,
                Provedor = usuario.Provedor,
                DataUltimoLogin = usuario.DataUltimoLogin
            };

            return Ok(usuarioDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno no servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Desativa usuário (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Desativar(Guid id)
    {
        try
        {
            var usuarioAtual = ObterUsuarioAtual();
            
            // Usuário só pode desativar sua própria conta, exceto admins
            if (usuarioAtual.Id != id && !User.IsInRole("Admin"))
            {
                return Forbid("Você só pode desativar sua própria conta");
            }

            var usuario = await _usuarioRepository.ObterPorIdAsync(id);
            
            if (usuario == null)
            {
                return NotFound(new { message = "Usuário não encontrado" });
            }

            // Soft delete - apenas desativa
            usuario.Ativo = false;
            usuario.DataAtualizacao = DateTime.UtcNow;

            await _usuarioRepository.AtualizarAsync(usuario);

            return Ok(new { message = "Usuário desativado com sucesso" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno no servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtém perfil do usuário autenticado
    /// </summary>
    [HttpGet("perfil")]
    public async Task<IActionResult> ObterPerfil()
    {
        try
        {
            var usuarioAtual = ObterUsuarioAtual();
            var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioAtual.Id);
            
            if (usuario == null)
            {
                return NotFound(new { message = "Usuário não encontrado" });
            }

            var usuarioDto = new UsuarioPerfilDto
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                AvatarUrl = usuario.AvatarUrl,
                Provedor = usuario.Provedor,
                DataUltimoLogin = usuario.DataUltimoLogin,
                DataCriacao = usuario.DataCriacao,
                Ativo = usuario.Ativo
            };

            return Ok(usuarioDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro interno no servidor", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtém dados do usuário atual a partir do token
    /// </summary>
    private UsuarioTokenInfo ObterUsuarioAtual()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var nome = User.FindFirst(ClaimTypes.Name)?.Value;

        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            throw new UnauthorizedAccessException("Token inválido");
        }

        return new UsuarioTokenInfo
        {
            Id = userGuid,
            Email = email ?? "",
            Nome = nome ?? ""
        };
    }
}

/// <summary>
/// DTO para atualização de usuário
/// </summary>
public class AtualizarUsuarioDto
{
    /// <summary>
    /// Nome do usuário
    /// </summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Email do usuário
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// URL do avatar
    /// </summary>
    public string? AvatarUrl { get; set; }
}

/// <summary>
/// DTO para perfil completo do usuário
/// </summary>
public class UsuarioPerfilDto : UsuarioDto
{
    /// <summary>
    /// Data de criação da conta
    /// </summary>
    public DateTime DataCriacao { get; set; }

    /// <summary>
    /// Status da conta
    /// </summary>
    public bool Ativo { get; set; }
}

/// <summary>
/// Informações do usuário extraídas do token
/// </summary>
public class UsuarioTokenInfo
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
}
