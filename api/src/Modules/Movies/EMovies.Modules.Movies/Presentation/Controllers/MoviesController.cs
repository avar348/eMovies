using EMovies.Modules.Movies.Application;
using EMovies.Modules.Movies.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EMovies.Modules.Movies.Presentation.Controllers;

[ApiController]
[Route("api/movies")]
[Authorize]
public sealed class MoviesController(IMovieService movieService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = MoviesPolicies.Read)]
    [ProducesResponseType<IReadOnlyList<MovieResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MovieResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        return Ok(await movieService.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = MoviesPolicies.Read)]
    [ProducesResponseType<MovieResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MovieResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var movie = await movieService.GetByIdAsync(id, cancellationToken);
        return movie is null ? NotFound() : Ok(movie);
    }

    [HttpPost]
    [Authorize(Policy = MoviesPolicies.Write)]
    [ProducesResponseType<MovieResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MovieResponse>> Create(
        CreateMovieRequest request,
        CancellationToken cancellationToken)
    {
        var movie = await movieService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = movie.Id }, movie);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = MoviesPolicies.Write)]
    [ProducesResponseType<MovieResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MovieResponse>> Update(
        Guid id,
        UpdateMovieRequest request,
        CancellationToken cancellationToken)
    {
        var movie = await movieService.UpdateAsync(id, request, cancellationToken);
        return movie is null ? NotFound() : Ok(movie);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = MoviesPolicies.Write)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await movieService.DeleteAsync(id, cancellationToken)
            ? NoContent()
            : NotFound();
    }
}
