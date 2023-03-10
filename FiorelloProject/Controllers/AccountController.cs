using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FiorelloProject.Helpers;
using FiorelloProject.Models;
using FiorelloProject.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FiorelloProject.Controllers
{
    public class AccountController : Controller
     
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }



        public IActionResult Register()
        {
            return View();
        }



        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async  Task<IActionResult> Register(RegisterVM register)
        {
            if (!ModelState.IsValid) return View();

            AppUser user = new();
            //user.Id = Guid.NewGuid().ToString();
            user.Fulname =register.Fulname;
            user.UserName = register.Username;
            user.Email = register.Email;
    

            IdentityResult result = await _userManager.CreateAsync(user, register.Password);

            if(!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);

                }
                return View(register);
            }

            await _userManager.AddToRoleAsync(user, RoleEnums.Admin.ToString());

            return RedirectToAction("login");
        }


        public IActionResult Login()
        {
            return View();
        }



        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Login(LoginVM login, string ReturnUrl)
        {

            if (!ModelState.IsValid) return View();
            AppUser user = await _userManager.FindByEmailAsync(login.UsernameOrEmail);
            if (user == null)
            {
                user =await _userManager.FindByEmailAsync(login.UsernameOrEmail);
                if (user == null)
                {
                    ModelState.AddModelError("", "Username,Email or password invalid");
                    return View(login);
                }
            }

            var result =await _signInManager.PasswordSignInAsync(user, login.Password, login.RememberMe, true);

            if (result.IsLockedOut)
            {
                ModelState.AddModelError("", "hasabiniz bloklanib");
                return View(login);
            }

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Username,Email or password invalid");
                return View(login);
            }



            //if (await _userManager.IsInRoleAsync(user, RoleEnums.Admin.ToString()))
            //{


            //    return RedirectToAction("index", "dashboard", new { Area = "AdminArea" });
            //}

            if (ReturnUrl!=null)
            {
                return Redirect(ReturnUrl);
            }


            await _signInManager.SignInAsync(user, true);


            return RedirectToAction("index", "home");
        }



        public async  Task<IActionResult> Logout()
        {

           await _signInManager.SignOutAsync();

            return RedirectToAction("login");
        }





        //public async Task<IActionResult> CreateRole()
        //{
        //    foreach (var item in Enum.GetValues(typeof(RoleEnums)))
        //    {

        //        if (!await _roleManager.RoleExistsAsync(item.ToString()))
        //        {
        //            await _roleManager.CreateAsync(new IdentityRole { Name = item.ToString() });
        //        }

        //    }
        //    return Content("role added");
        //}

    }
}