using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdminRazer.Models;
using AdminRazer.ViewModels;
using AdminRazer.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace AdminRazer.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ClientesController : Controller
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly UserManager<IdentityUser> _userManager;

        public ClientesController(IClienteRepository clienteRepository, UserManager<IdentityUser> userManager)
        {
            _clienteRepository = clienteRepository;
            _userManager = userManager;
        }

        // GET: Clientes
        public async Task<IActionResult> Index()
        {
            var clientes = await _clienteRepository.GetAllAsync();
            return View(clientes);
        }

        // GET: Clientes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var cliente = await _clienteRepository.GetByIdAsync(id.Value);
            if (cliente == null) return NotFound();

            // Mapear la entidad Cliente al ClienteEditViewModel esperado por la vista Details
            var vm = new ClienteEditViewModel
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre,
                Documento = cliente.Documento,
                Telefono = cliente.Telefono,
                Email = cliente.Email
            };

            return View(vm);
        }

        // GET: Clientes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Clientes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClienteCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var cliente = new Cliente
                {
                    Nombre = model.Nombre,
                    Documento = model.Documento,
                    Telefono = model.Telefono,
                    Email = model.Email
                };

                // Crear usuario Identity y enlazar
                if (!string.IsNullOrWhiteSpace(model.Email))
                {
                    var existingUser = await _userManager.FindByEmailAsync(model.Email);
                    if (existingUser == null)
                    {
                        var identityUser = new IdentityUser
                        {
                            UserName = model.Email,
                            Email = model.Email,
                            EmailConfirmed = true
                        };

                        var createResult = await _userManager.CreateAsync(identityUser, model.Password);
                        if (createResult.Succeeded)
                        {
                            await _userManager.AddToRoleAsync(identityUser, "Cliente");
                            // recargar desde store para obtener PasswordHash actualizado
                            var persisted = await _userManager.FindByIdAsync(identityUser.Id);
                            if (persisted != null)
                            {
                                cliente.IdentityUserId = persisted.Id;
                                cliente.PasswordHash = persisted.PasswordHash;
                            }
                            else
                            {
                                // Fallback: usar el objeto en memoria si por alguna razón no se pudo recargar
                                cliente.IdentityUserId = identityUser.Id;
                                cliente.PasswordHash = identityUser.PasswordHash;
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, string.Join(';', createResult.Errors.Select(e => e.Description)));
                            return View(model);
                        }
                    }
                    else
                    {
                        // usuario ya existe: enlazar
                        cliente.IdentityUserId = existingUser.Id;
                        cliente.PasswordHash = existingUser.PasswordHash;
                    }
                }

                await _clienteRepository.AddAsync(cliente);
                await _clienteRepository.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Clientes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var cliente = await _clienteRepository.GetByIdAsync(id.Value);
            if (cliente == null) return NotFound();

            var vm = new ClienteEditViewModel
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre,
                Documento = cliente.Documento,
                Telefono = cliente.Telefono,
                Email = cliente.Email
            };

            return View(vm);
        }

        // POST: Clientes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ClienteEditViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var cliente = await _clienteRepository.GetByIdAsync(model.Id);
                    if (cliente == null) return NotFound();

                    cliente.Nombre = model.Nombre;
                    cliente.Documento = model.Documento;
                    cliente.Telefono = model.Telefono;
                    cliente.Email = model.Email;

                    // Si se proporcionó una nueva contraseña, actualizar la del usuario Identity asociado
                    if (!string.IsNullOrWhiteSpace(model.Password))
                    {
                        IdentityUser? identityUser = null;

                        if (!string.IsNullOrWhiteSpace(cliente.IdentityUserId))
                        {
                            identityUser = await _userManager.FindByIdAsync(cliente.IdentityUserId);
                        }

                        // si no hay IdentityUser enlazado, intentar por Email
                        if (identityUser == null && !string.IsNullOrWhiteSpace(cliente.Email))
                        {
                            identityUser = await _userManager.FindByEmailAsync(cliente.Email);
                        }

                        if (identityUser != null)
                        {
                            // Resetear contraseña usando token
                            var token = await _userManager.GeneratePasswordResetTokenAsync(identityUser);
                            var resetResult = await _userManager.ResetPasswordAsync(identityUser, token, model.Password);
                            if (resetResult.Succeeded)
                            {
                                // recargar para obtener el nuevo hash
                                var reloaded = await _userManager.FindByIdAsync(identityUser.Id);
                                if (reloaded != null)
                                {
                                    cliente.PasswordHash = reloaded.PasswordHash;
                                }
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, string.Join(';', resetResult.Errors.Select(e => e.Description)));
                                return View(model);
                            }
                        }
                        else
                        {
                            // No se encontró IdentityUser: crear uno nuevo y enlazar
                            if (!string.IsNullOrWhiteSpace(cliente.Email))
                            {
                                var newUser = new IdentityUser { UserName = cliente.Email, Email = cliente.Email, EmailConfirmed = true };
                                var createResult = await _userManager.CreateAsync(newUser, model.Password);
                                if (createResult.Succeeded)
                                {
                                    await _userManager.AddToRoleAsync(newUser, "Cliente");
                                    // recargar para obtener PasswordHash
                                    var persistedNew = await _userManager.FindByIdAsync(newUser.Id);
                                    if (persistedNew != null)
                                    {
                                        cliente.IdentityUserId = persistedNew.Id;
                                        cliente.PasswordHash = persistedNew.PasswordHash;
                                    }
                                    else
                                    {
                                        cliente.IdentityUserId = newUser.Id;
                                        cliente.PasswordHash = newUser.PasswordHash;
                                    }
                                }
                                else
                                {
                                    ModelState.AddModelError(string.Empty, string.Join(';', createResult.Errors.Select(e => e.Description)));
                                    return View(model);
                                }
                            }
                        }
                    }

                    _clienteRepository.Update(cliente);
                    await _clienteRepository.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _clienteRepository.AnyAsync(e => e.Id == model.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Clientes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated || !User.IsInRole("Administrador"))
            {
                return RedirectToAction("Index", "Home");
            }
            if (id == null) return NotFound();

            var cliente = await _clienteRepository.GetByIdAsync(id.Value);
            if (cliente == null) return NotFound();

            var vm = new ClienteEditViewModel
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre,
                Documento = cliente.Documento,
                Telefono = cliente.Telefono,
                Email = cliente.Email
            };

            return View(vm);
        }

        // POST: Clientes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated || !User.IsInRole("Administrador"))
            {
                return RedirectToAction("Index", "Home");
            }
            var cliente = await _clienteRepository.GetByIdAsync(id);
            if (cliente != null)
            {
                _clienteRepository.Remove(cliente);
                await _clienteRepository.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
