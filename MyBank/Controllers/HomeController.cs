using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using MyBank.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MyBank.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext _bank { get; set; }

        private ApplicationUserManager _userManager { get { return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); } }

        public HomeController()
        {
            _bank = new ApplicationDbContext();
        }

        /// <summary>
        /// Lists all accounts the of current users.
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public async Task<ActionResult> Index(string message = "", bool error = false, bool success = false)
        {
            var user = await _userManager.FindByIdAsync(User.Identity.GetUserId());
            var userId = user.Id;

            var model = new AccountOverviewViewModel
            {
                Accounts = _bank.Accounts.Where(account => account.OwnerId == userId).ToList(),
                Message = message,
                Error = error,
                Success = success
            };

            return View(model);
        }

        /// <summary>
        /// Create a new account for the currently logged in user.
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public RedirectToRouteResult Create()
        {
            // If there are already accounts, find the highest account number of all and add 1, otherwise take 1:
            var accountNumber = _bank.Accounts.Any() ? _bank.Accounts.Max(account => account.Number) + 1 : 1;

            // Create a new account with the current UserId as owner and the account number just found:
            var newAccount = new BankAccount(User.Identity.GetUserId(), accountNumber);

            // Add the account to the bank's list of accounts:
            _bank.Accounts.Add(newAccount);

            // Save the changes to the database:
            _bank.SaveChanges();

            // Display the overview page (index):
            return RedirectToAction("Index", new { message = "The account number " + accountNumber + " was created successfully. ", success = true });
        }

        /// <summary>
        /// Displays the view for deposit.
        /// </summary>
        /// <param name="account">account number</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public ActionResult PayIn(int account)
        {
            return View(account);
        }

        /// <summary>
        /// Method to make the deposit. Must be called via POST!
        /// </summary>
        /// <param name="account">account number</param>
        /// <param name="amount">amount</param>
        /// <returns>Returns to index</returns>
        [HttpPost]
        [Authorize]
        public RedirectToRouteResult PayIn(int account, double amount)
        {
            var myAccount = _bank.Accounts.FirstOrDefault(acc => acc.Number == account);
            if (myAccount == null)
                return RedirectToAction("Index", new { message = "Account not found ", error = true });

            myAccount.PayIn(amount);
            _bank.SaveChanges();

            return RedirectToAction("Index", new { message = "Amount successfully deposited. ", success = true });
        }

        /// <summary>
        /// Displays the view for withdrawal.
        /// </summary>
        /// <param name="account">account number</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public ActionResult Withdraw(int account)
        {
            return View(account);
        }

        /// <summary>
        /// Method to perform the withdrawal. Must be called via POST!
        /// </summary>
        /// <param name="account">account number</param>
        /// <param name="amount">amount</param>
        /// <returns> Returns to index</returns>
        [HttpPost]
        [Authorize]
        public RedirectToRouteResult Withdraw(int account, double amount)
        {
            var myAccount = _bank.Accounts.FirstOrDefault(acc => acc.Number == account);
            if (myAccount != null)
            {
                if (!myAccount.Withdraw(amount))
                    return RedirectToAction("Index", new { message = "Account coverage is insufficient. ", error = true });
            }
            else
                return RedirectToAction("Index", new { message = "Account not found. ", error = true });

            _bank.SaveChanges();
            return RedirectToAction("Index", new { message = "Amount successfully unsaved. ", success = true });
        }

        /// <summary>
        ///  Displays the view for transfer.
        /// </summary>
        /// <param name="account">account number</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public ActionResult Transfer(int account)
        {
            var model = new TransferViewModel { From = account };
            return View(model);
        }

        /// <summary>
        /// Method to make the transfer. Must be called via POST!
        /// </summary>
        /// <param name="from">Payer</param>
        /// <param name="to">Payee </param>
        /// <param name="amount">amount </param>
        /// <returns>Returns to index</returns>
        [HttpPost]
        [Authorize]
        public RedirectToRouteResult Transfer(TransferViewModel model)
        {
            if (ModelState.IsValid)
            {
                var sender = _bank.Accounts.FirstOrDefault(acc => acc.Number == model.From);
                var receiver = _bank.Accounts.FirstOrDefault(acc => acc.Number == model.To);
                if (sender != null && receiver != null)
                {
                    if (!sender.Transfer(model.Amount, receiver))
                        return RedirectToAction("Index", new { message = "Account coverage is insufficient. ", error = true });
                }
                else
                    return RedirectToAction("Index", new { message = "Account not found. ", error = true });

                _bank.SaveChanges();
            }
            return RedirectToAction("Index", new { message = "Amount successfully transferred. ", success = true });
        }
    }
}