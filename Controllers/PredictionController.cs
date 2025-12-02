using Cadar_Raul_Lab4.Data;
using Cadar_Raul_Lab4.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;

namespace Cadar_Raul_Lab4.Controllers
{
    public class PredictionController : Controller
    {
        private readonly AppDbContext _context;
        
        public PredictionController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult Price()
        {
            return View(new PricePredictionModel.ModelInput());
        }
        [HttpPost]

        public async Task<IActionResult> Price(PricePredictionModel.ModelInput input)
        {
            // Load the model
            MLContext mlContext = new MLContext();
            // Create predection engine related to the loaded train model
            ITransformer mlModel = mlContext.Model.Load(@"..\Cadar_Raul_Lab4\PricePredictionModel.mlnet", out var modelInputSchema);
            var predEngine = mlContext.Model.CreatePredictionEngine<
                PricePredictionModel.ModelInput,
                PricePredictionModel.ModelOutput>(mlModel);
            // Try model on sample data to predict fair price
            PricePredictionModel.ModelOutput result = predEngine.Predict(input);
            ViewBag.Price = result.Score;

            if (!string.IsNullOrEmpty(input.Payment_type))
            {

                var history = new PredictionHistory
                {
                    PassengerCount = input.Passenger_count,
                    TripTimeInSecs = input.Trip_time_in_secs,
                    TripDistance = input.Trip_distance,
                    PaymentType = input.Payment_type ?? "N/A",
                    PredictedPrice = result.Score,
                    CreatedAt = DateTime.Now
                };
                _context.PredictionHistories.Add(history);
                await _context.SaveChangesAsync();
                ViewBag.HistoryMessage = "Prediction saved to history.";
            }

            else
            {
                ViewBag.HistoryMessage = "Prediction not saved due to missing Payment Type.";
            }

            return View(input);
        }
        //public async Task<IActionResult> History()
        //{
        //    var history = await _context.PredictionHistories
        //    .OrderByDescending(p => p.CreatedAt)
        //    .ToListAsync();
        //    return View(history);
        //}

        [HttpGet]
        public async Task<IActionResult> History(
            string? paymentType,
            float? minPrice,
            float? maxPrice,
            string? sortOrder,
            DateTime? minDate,
            DateTime? maxDate)
        {
            var query = _context.PredictionHistories.AsQueryable();
            if (!string.IsNullOrEmpty(paymentType))
            {
                query = query.Where(p => p.PaymentType == paymentType);
            }
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.PredictedPrice >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.PredictedPrice <= maxPrice.Value);
            }
            if (minDate.HasValue)
            {
                query = query.Where(p => p.CreatedAt.Date >= minDate.Value.Date);
            }
            if (maxDate.HasValue)
            {
                query = query.Where(p => p.CreatedAt.Date <= maxDate.Value.Date);
            }
            query = sortOrder switch
            {
                "price_asc" => query.OrderBy(p => p.PredictedPrice),
                "price_desc" => query.OrderByDescending(p => p.PredictedPrice),
                "date_asc" => query.OrderBy(p => p.CreatedAt),
                "date_desc" => query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderBy(p => p.PredictedPrice) //sortare default
            };
            ViewBag.CurrentPaymentType = paymentType;
            ViewBag.CurrentMinPrice = minPrice;
            ViewBag.CurrentMaxPrice = maxPrice;
            ViewBag.CurrentSortOrder = sortOrder;
            ViewBag.CurrentMinDate = minDate?.ToString("yyyy-MM-dd");
            ViewBag.CurrentMinDate = maxDate?.ToString("yyyy-MM-dd");
            var result = await query.ToListAsync();
            return View(result);
        }

        public IActionResult Time(DurationPredictionModel.ModelInput input)
        {
            // Load the model
            MLContext mlContext = new MLContext();
            // Create predection engine related to the loaded train model
            ITransformer mlModel =
           mlContext.Model.Load(@"..\Cadar_Raul_Lab4\DurationPredictionModel.mlnet", out var modelInputSchema);
            var predEngine = mlContext.Model.CreatePredictionEngine<DurationPredictionModel.ModelInput,
           DurationPredictionModel.ModelOutput>(mlModel);
            // Try model on sample data to predict fair time
            DurationPredictionModel.ModelOutput result = predEngine.Predict(input);
            ViewBag.Time = result.Score;
            return View(input);
        }
    }
}
