﻿using Diabot.Services.Concrete;
using Diabot.Services.Interfaces;
using Diabot.ViewModels.Meals;
using Diabot.ViewModels.Scheduler;
using Diabot.ViewModels.Home;
using Diabot.Views.Meals;
using Diabot.Views.Scheduler;
using Diabot.Views.Home;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;

namespace Diabot
{
    public static class MauiProgram
    {
        private const string _cosmosEndpoint = "https://diabetes-recipes-db.documents.azure.com:443/";
        private const string _cosmosKey = "<Enter CosmosDB Key Here>";

        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.ConfigureSyncfusionCore();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            Task.Run(InitCosmosDb);
            builder = RegisterViews(builder);
            builder = RegisterViewModels(builder);
            builder = RegisterServices(builder);
            builder = RegisterOthers(builder);
#if DEBUG
            builder.Logging.AddDebug();
#endif
            return builder.Build();
        }

        private static MauiAppBuilder RegisterViews(MauiAppBuilder builder)
        {
            // Meals
            builder.Services.AddSingleton<MealsPage>();
            builder.Services.AddTransient<MealDetailsPage>();
            builder.Services.AddTransient<AddMealPage>();
            builder.Services.AddTransient<EditMealPage>();
            
            // Scheduler
            builder.Services.AddSingleton<SchedulerPage>();
            builder.Services.AddTransient<ScheduleItemDetailsPage>();
            builder.Services.AddTransient<AddScheduleItemPage>();
            builder.Services.AddTransient<EditScheduleItemPage>();
            
            // Home
            builder.Services.AddSingleton<HomePage>();
            return builder;
        }
        
        private static MauiAppBuilder RegisterViewModels(MauiAppBuilder builder)
        {
            // Meals
            builder.Services.AddSingleton<MealsViewModel>();
            builder.Services.AddTransient<MealDetailsViewModel>();
            builder.Services.AddTransient<AddMealViewModel>();
            builder.Services.AddTransient<EditMealViewModel>();

            // Scheduler
            builder.Services.AddSingleton<SchedulerViewModel>();
            builder.Services.AddTransient<ScheduleItemDetailsViewModel>();
            builder.Services.AddTransient<AddScheduleItemViewModel>();
            builder.Services.AddTransient<EditScheduleItemViewModel>();

            // Home
            builder.Services.AddSingleton<HomeViewModel>();

            return builder;
        }
        
        private static MauiAppBuilder RegisterServices(MauiAppBuilder builder)
        {
            builder.Services.AddSingleton<IMealService, CosmosMealService>();
            builder.Services.AddSingleton<ISchedulerService, CosmosSchedulerService>();
            return builder;
        }

        private static MauiAppBuilder RegisterOthers(MauiAppBuilder builder)
        {
            // Cosmos DB
            builder.Services.AddSingleton(config =>
            {
                var client = new CosmosClient(_cosmosEndpoint, _cosmosKey);
                return client;
            });
            builder.Services.AddSingleton(Connectivity.Current);
            return builder;
        }

        private static async Task InitCosmosDb()
        {
            var client = new CosmosClient(_cosmosEndpoint, _cosmosKey);
            try
            {
                Database db = await client.CreateDatabaseIfNotExistsAsync(
                        id: "diabot-db"
                    );
                await db.CreateContainerIfNotExistsAsync(
                    id: "meals",
                    partitionKeyPath: "/meals",
                    throughput: 400
                );
                await db.CreateContainerIfNotExistsAsync(
                    id: "schedules",
                    partitionKeyPath: "/schedules",
                    throughput: 400
                );
            } 
            catch (Exception ex) 
            {
                await Shell.Current.DisplayAlert("Cosmos Error", $"Unable to connect to CosmosDB. {ex.Message}", "Quit");
            }
        }
    }
}