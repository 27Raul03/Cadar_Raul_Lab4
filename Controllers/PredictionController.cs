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

            return View(input);
        }
        public async Task<IActionResult> History()
        {
            var history = await _context.PredictionHistories
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
            return View(history);
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
