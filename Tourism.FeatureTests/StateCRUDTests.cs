﻿using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Tourism.DataAccess;
using Tourism.Models;

namespace Tourism.FeatureTests
{
    public class StateCRUDTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public StateCRUDTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async void Index_ReturnsAllStates()
        {
            var context = GetDbContext();
            var client = _factory.CreateClient();

            context.States.Add(new State { Name = "Iowa", Abbreviation = "IA" });
            context.States.Add(new State { Name = "Colorado", Abbreviation = "CO" });
            context.SaveChanges();

            var response = await client.GetAsync("/states");
            var html = await response.Content.ReadAsStringAsync();

            Assert.Contains("IA", html);
            Assert.Contains("Iowa", html);
            Assert.Contains("CO", html);
            Assert.Contains("Colorado", html);
            Assert.DoesNotContain("California", html);
        }

        [Fact]
        public async void Show_ReturnsSingleState()
        {
            var context = GetDbContext();
            var client = _factory.CreateClient();

            context.States.Add(new State { Name = "Iowa", Abbreviation = "IA" });
            context.States.Add(new State { Name = "Colorado", Abbreviation = "CO" });
            context.SaveChanges();

            var response = await client.GetAsync("/states/1");
            var html = await response.Content.ReadAsStringAsync();

            Assert.Contains("Iowa", html);
            Assert.DoesNotContain("Colorado", html);
        }

        private TourismContext GetDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<TourismContext>();
            optionsBuilder.UseInMemoryDatabase("TestDatabase");

            var context = new TourismContext(optionsBuilder.Options);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            return context;
        }

        [Fact]
        public async Task Test_New_ReturnsForms()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/states/new");
            var html = await response.Content.ReadAsStringAsync();

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Contains("Make a new state", html);
            Assert.Contains("<form method=\"post\" action=\"/states\">", html);
        }

        [Fact]
        public async Task AddState_ReturnRedierectToShow()
        {
            var client = _factory.CreateClient();

            var formData = new Dictionary<string, string>
            {
                { "Name", "Colorado" },
                { "Abbreviation", "CO" }
            };

            // Act
            var response = await client.PostAsync("/states", new FormUrlEncodedContent(formData));
            var html = await response.Content.ReadAsStringAsync();

            // Assert that we are redirected to a details page for the movie just created
            response.EnsureSuccessStatusCode();
       
            Assert.Contains("Colorado", html);
            Assert.Contains("CO", html);
        }
    }
}
