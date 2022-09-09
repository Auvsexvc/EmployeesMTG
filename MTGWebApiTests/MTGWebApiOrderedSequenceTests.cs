using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using MTGWebApi.Controllers;
using MTGWebApi.Data;
using MTGWebApi.Enums;
using MTGWebApi.Interfaces;
using MTGWebApi.Models;
using MTGWebApi.Services;
using System.Reflection;

namespace MTGWebApiTests
{
    [TestFixture]
    public class MTGWebApiOrderedSequenceTests
    {
        private AppDbInitializer _appDbInitializer;
        private IConfiguration _configuration;
        private IAppDbFileHandler _appDbFileHandler;
        private IAppDbContext _appDbContext;
        private IEmployeeService _employeeService;
        private EmployeeController _employeeController;
        private string _dbFullPath;
        private string _tempFullPath;

        [OneTimeSetUp]
        public async Task Setup()
        {
            _configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
            _appDbFileHandler = new AppDbFileHandler();
            _appDbInitializer = new AppDbInitializer(_configuration, _appDbFileHandler);
            _appDbContext = new AppDbContext(_configuration, _appDbFileHandler);
            _employeeService = new EmployeeService(_appDbContext, new NullLogger<EmployeeService>());
            _employeeController = new EmployeeController(_employeeService);
            var dbFile = _configuration.GetSection("DbInfo:DbFile").Get<string>();
            var tempFile = _configuration.GetSection("DbInfo:TempFile").Get<string>();
            _dbFullPath = Path.Combine(Environment.CurrentDirectory, dbFile);
            _tempFullPath = Path.Combine(Environment.CurrentDirectory, tempFile);
            await Task.Run(() => File.Delete(_dbFullPath));
            await Task.Run(() => File.Delete(_tempFullPath));
        }

        [Test, Order(1)]
        public async Task DatabaseShouldBeCreatedOnFirstRunIfDoesNotExist()
        {
            await _appDbInitializer.StartAsync(default);

            Assert.That(_appDbContext.CanConnectToDb(), Is.True);
        }

        [Test, Order(2)]
        public async Task DatabaseShouldBeEmptyOnFirstRun()
        {
            IActionResult actionResult = await _employeeController.GetAll();
            Assert.That(actionResult, Is.TypeOf<OkObjectResult>());
            List<EmployeeVM>? actionResultData = (actionResult as OkObjectResult)?.Value as List<EmployeeVM>;

            Assert.That(actionResultData, Has.Count.EqualTo(0));
        }

        [Test, Order(3)]
        public async Task ThereShouldBeNoPendingChangesOnFirstRunAndGetChangesActionShouldReturnNoContentResult()
        {
            IActionResult actionResult = await _employeeController.GetChanges();
            Assert.That(actionResult, Is.TypeOf<NoContentResult>());
        }

        [Test, Order(4)]
        public async Task CreateActionTest()
        {
            var emplCreateDto = new CreateEmployeeDto()
            {
                FirstName = "Joe",
                LastName = "Smith",
                StreetName = "Short",
                HouseNumber = "3A",
                ApartmentNumber = "220",
                PostalCode = "00-213",
                Town = "Hello Town",
                PhoneNumber = "+48999999222",
                DateOfBirth = DateTime.Parse("1977 - 07 - 02")
            };
            var actionResult = await _employeeController.Create(emplCreateDto);
            Assert.That(actionResult, Is.TypeOf<CreatedResult>());
        }

        [Test, Order(5)]
        public async Task PendingChangesAfterCreateSholudBeOneTest()
        {
            IActionResult actionResult = await _employeeController.GetChanges();
            IEnumerable<EmployeeVM>? actionResultData = (actionResult as OkObjectResult)?.Value as IEnumerable<EmployeeVM>;
            Assert.That(actionResultData, Is.Not.Empty);
            Assert.That(actionResultData.Count, Is.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(actionResultData.First().Town, Is.EqualTo("Hello Town"));
                Assert.That(actionResultData.First().State, Is.EqualTo(Operation.Create));
            });
        }

        [Test, Order(6)]
        public async Task GetAllActionCountShouldBeOneTest()
        {
            IActionResult actionResult = await _employeeController.GetAll();
            Assert.That(actionResult, Is.TypeOf<OkObjectResult>());
            List<EmployeeVM>? actionResultData = (actionResult as OkObjectResult)?.Value as List<EmployeeVM>;

            Assert.That(actionResultData, Has.Count.EqualTo(1));
        }

        [Test, Order(7)]
        public async Task GetByIdActionTest()
        {
            IActionResult actionResult = await _employeeController.GetAll();
            Assert.That(actionResult, Is.TypeOf<OkObjectResult>());
            IEnumerable<EmployeeVM>? actionResultData = (actionResult as OkObjectResult)?.Value as IEnumerable<EmployeeVM>;
            Guid idToFind = actionResultData!.First().Id;
            IActionResult getByIdActionResult = await _employeeController.GetById(idToFind);
            Assert.That(getByIdActionResult, Is.TypeOf<OkObjectResult>());
            EmployeeVM? getByIdResultDate = (getByIdActionResult as OkObjectResult)?.Value as EmployeeVM;
            Assert.That(getByIdResultDate!.FirstName, Is.EqualTo("Joe"));
        }

        [Test, Order(8)]
        public async Task UpdateWithLeapYearDOBActionTest()
        {
            var employee = await _employeeService.GetAllAsync("asc", searchString: "Joe", 1, 0);

            var emplUpdateDto = new UpdateEmployeeDto()
            {
                FirstName = "Jane",
                LastName = "Smith",
                StreetName = "Long",
                HouseNumber = "4A",
                ApartmentNumber = "1",
                PostalCode = "11-213",
                Town = "Kitty Village",
                PhoneNumber = "+23777666222",
                DateOfBirth = DateTime.Parse("2012 - 02 - 29")
            };

            var actionResult = await _employeeController.Update(employee.First().Id, emplUpdateDto);
            Assert.That(actionResult, Is.TypeOf<OkResult>());

            var searchforJoe = (await _employeeService.GetAllAsync("asc", searchString: "Joe", 1, 0)).Count();
            Assert.That(searchforJoe, Is.EqualTo(0));

            var searchforJane = (await _employeeService.GetAllAsync("asc", searchString: "Jane", 1, 0)).Count();
            Assert.That(searchforJane, Is.EqualTo(1));
        }

        [Test, Order(9)]
        public async Task PendingChangesAfterCreateAndThenUpdateOfCreatedShouldBeStillOneAndItsStateShouldBeCreateTest()
        {
            IActionResult actionResult = await _employeeController.GetChanges();
            IEnumerable<EmployeeVM>? actionResultData = (actionResult as OkObjectResult)?.Value as IEnumerable<EmployeeVM>;
            Assert.That(actionResultData, Is.Not.Empty);
            Assert.That(actionResultData.Count, Is.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(actionResultData.First().Town, Is.Not.EqualTo("Hello Town"));
                Assert.That(actionResultData.First().State, Is.Not.EqualTo(Operation.Update));
            });

            Assert.Multiple(() =>
            {
                Assert.That(actionResultData.First().Town, Is.EqualTo("Kitty Village"));
                Assert.That(actionResultData.First().State, Is.EqualTo(Operation.Create));
            });
        }

        [Test, Order(10)]
        public async Task AgeIsReadOnlyTest()
        {
            IActionResult actionResult = await _employeeController.GetAll();
            IEnumerable<EmployeeVM>? actionResultData = (actionResult as OkObjectResult)?.Value as IEnumerable<EmployeeVM>;
            Guid idToFind = actionResultData!.First().Id;
            IActionResult getByIdActionResult = await _employeeController.GetById(idToFind);
            EmployeeVM? getByIdResultDate = (getByIdActionResult as OkObjectResult)?.Value as EmployeeVM;

            PropertyInfo? agePropertyInfo = getByIdResultDate!.GetType().GetProperty("Age");
            MethodInfo? getMethod = agePropertyInfo!.GetGetMethod();
            MethodInfo? setMethod = agePropertyInfo.GetSetMethod();

            Assert.Multiple(() =>
            {
                Assert.That(getMethod, Is.Not.Null);
                Assert.That(getMethod!.IsPublic, Is.True);
            });

            Assert.That(setMethod, Is.Null);
        }

        [Test, Order(11)]
        public async Task SaveChangesActionTest()
        {
            await _employeeController.Save();
            IActionResult actionResult = await _employeeController.GetAll();
            IEnumerable<EmployeeVM>? actionResultData = (actionResult as OkObjectResult)?.Value as IEnumerable<EmployeeVM>;
            var getAllResultsCount = actionResultData!.Count();
            IActionResult getPendingActionResult = await _employeeController.GetChanges();

            Assert.Multiple(() =>
            {
                Assert.That(getAllResultsCount, Is.EqualTo(1));
                Assert.That(getPendingActionResult, Is.TypeOf<NoContentResult>());
            });
        }

        [Test, Order(12)]
        public async Task DeleteActionTest()
        {
            IActionResult actionResult = await _employeeController.GetAll();
            IEnumerable<EmployeeVM>? actionResultData = (actionResult as OkObjectResult)?.Value as IEnumerable<EmployeeVM>;
            Guid idToFind = actionResultData!.First().Id;

            var deleteResult = await _employeeController.Delete(idToFind);

            Assert.That(deleteResult, Is.TypeOf<NoContentResult>());
        }

        [Test, Order(13)]
        public async Task CancelActionTest()
        {
            IActionResult actionResult = await _employeeController.Cancel();
            Assert.That(actionResult, Is.TypeOf<OkResult>());

            IActionResult getChangesResult = await _employeeController.GetChanges();
            Assert.That(getChangesResult, Is.TypeOf<NoContentResult>());
        }

        [Test, Order(14)]
        public async Task CreateAndItsDeletionShouldProduceNoPendingChangesTest()
        {
            var emplCreateDto = new CreateEmployeeDto()
            {
                FirstName = "Joe",
                LastName = "Smith",
                StreetName = "Short",
                HouseNumber = "3A",
                ApartmentNumber = "220",
                PostalCode = "00-213",
                Town = "Hello Town",
                PhoneNumber = "+48999999222",
                DateOfBirth = DateTime.Parse("1977 - 07 - 02")
            };
            var createAction = await _employeeController.Create(emplCreateDto);
            Assert.That(createAction, Is.TypeOf<CreatedResult>());

            IActionResult actionResult = await _employeeController.GetAll("asc", searchString: emplCreateDto.FirstName, 1, 0);
            IEnumerable<EmployeeVM>? actionResultData = (actionResult as OkObjectResult)?.Value as IEnumerable<EmployeeVM>;
            Guid idToFind = actionResultData!.First().Id;

            var deleteResult = await _employeeController.Delete(idToFind);
            Assert.That(deleteResult, Is.TypeOf<NoContentResult>());

            IActionResult getChangesResult = await _employeeController.GetChanges();
            Assert.That(getChangesResult, Is.TypeOf<NoContentResult>());
        }

        [Test, Order(15)]
        public async Task Update1Create2Update2Update1Delete2Delete1Create3ShouldProduce2PendingChanges()
        {
            await _employeeController.Cancel();

            IActionResult actionResult1 = await _employeeController.GetAll("asc", searchString: "Jane", 1, 0);
            IEnumerable<EmployeeVM>? actionResultData1 = (actionResult1 as OkObjectResult)?.Value as IEnumerable<EmployeeVM>;

            var emplUpdateDto1 = new UpdateEmployeeDto()
            {
                FirstName = "Jane",
                LastName = "Smith",
                StreetName = "Trix Paolo",
                HouseNumber = "1",
                PostalCode = "90-213",
                Town = "BravoVille",
                PhoneNumber = "+23777666222",
                DateOfBirth = DateTime.Parse("2012 - 02 - 29")
            };
            await _employeeController.Update(actionResultData1!.First().Id, emplUpdateDto1);

            var emplCreateDto2 = new CreateEmployeeDto()
            {
                FirstName = "Adam",
                LastName = "Stevens",
                StreetName = "Jackson Street",
                HouseNumber = "190",
                PostalCode = "00-213",
                Town = "Hello Town",
                PhoneNumber = "+48999999222",
                DateOfBirth = DateTime.Parse("1922 - 01 - 01")
            };
            await _employeeController.Create(emplCreateDto2);

            IActionResult actionResult2 = await _employeeController.GetAll("asc", searchString: "Adam", 1, 0);
            IEnumerable<EmployeeVM>? actionResultData2 = (actionResult2 as OkObjectResult)?.Value as IEnumerable<EmployeeVM>;

            var emplUpdateDto2 = new UpdateEmployeeDto()
            {
                FirstName = "Adam",
                LastName = "Stevens",
                StreetName = "Round Street",
                HouseNumber = "1",
                PostalCode = "00-213",
                Town = "Little Town",
                PhoneNumber = "+44543539222",
                DateOfBirth = DateTime.Parse("1922 - 01 - 01")
            };
            await _employeeController.Update(actionResultData2!.First().Id, emplUpdateDto2);
            emplUpdateDto1.FirstName = "Nina";
            await _employeeController.Update(actionResultData1!.First().Id, emplUpdateDto1);

            await _employeeController.Delete(actionResultData2!.First().Id);
            await _employeeController.Delete(actionResultData1!.First().Id);

            var emplCreateDto3 = new CreateEmployeeDto()
            {
                FirstName = "Robert",
                LastName = "Angel",
                StreetName = "Simple Street",
                HouseNumber = "222",
                PostalCode = "33-213",
                Town = "MarinaBay",
                PhoneNumber = "+0909999222",
                DateOfBirth = DateTime.Parse("1999 - 02 - 01")
            };
            await _employeeController.Create(emplCreateDto3);

            var pendingChanges = await _employeeController.GetChanges();
            IEnumerable<EmployeeVM>? pendingChangesData = (pendingChanges as OkObjectResult)?.Value as IEnumerable<EmployeeVM>;
            Assert.That(pendingChangesData!.Count(), Is.EqualTo(2));
        }

        [Test, Order(16)]
        public async Task GetAllActionShouldShowOnlyRobert()
        {
            var actionResult = await _employeeController.GetAll();
            IEnumerable<EmployeeVM>? actionResultData = (actionResult as OkObjectResult)?.Value as IEnumerable<EmployeeVM>;
            Assert.Multiple(() =>
            {
                Assert.That(actionResultData!.Count(), Is.EqualTo(1));
                Assert.That(actionResultData!.First().FirstName, Is.EqualTo("Robert"));
            });
        }

        [Test, Order(17)]
        public async Task CancelShouldRevertToJaneFromKittyVilageOnly()
        {
            await _employeeController.Cancel();
            var actionResult = await _employeeController.GetAll();
            IEnumerable<EmployeeVM>? actionResultData = (actionResult as OkObjectResult)?.Value as IEnumerable<EmployeeVM>;
            Assert.Multiple(() =>
            {
                Assert.That(actionResultData!.Count(), Is.EqualTo(1));
                Assert.That(actionResultData!.First().FirstName, Is.EqualTo("Jane"));
                Assert.That(actionResultData!.First().Town, Is.EqualTo("Kitty Village"));
            });
        }
        [Test, Order(18)]
        public async Task ClearPendingChangesOnApiShutDown()
        {
            await _appDbInitializer.StopAsync(default);

            Assert.That(_appDbFileHandler.DoesFileExists(_tempFullPath), Is.False);
        }

        [OneTimeTearDown]
        public async Task CleanUp()

        {
            await Task.Run(() => File.Delete(_dbFullPath));
            await Task.Run(() => File.Delete(_tempFullPath));
        }
    }
}