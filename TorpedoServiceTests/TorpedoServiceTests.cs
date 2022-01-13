using Microsoft.VisualStudio.TestTools.UnitTesting;
using NationalInstruments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable NI1007 // Test classes must ultimately inherit from 'AutoTest'
#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable SA1000 // Keywords should be spaced correctly
#pragma warning disable CS8602 // Dereference of a possibly null reference.

// The study material also has them.
#pragma warning disable CA1707 // Identifiers should not contain underscores

namespace NationalInstruments.Tests
{
    [TestClass]
    public class TorpedoServiceTests
    {
        [TestMethod]
        public void HitSuggested_DetectPreviousHit_HitsAdjacent()
        {
            // Arrange
            TorpedoService ts = new(new InMemoryDataStore(), (9, 9));
            ts.StartingShips.Clear();
            ts.StartingShips.Add(new Ship(1) { Orientation = EOrientation.Right }, 1);
            ts.StartingShips.Add(new Ship(2) { Orientation = EOrientation.Right }, 1);

            ts.AddPlayer(new Player("a"));
            ts.AddPlayer(new Player("b"));
            ts.FinishAddingPlayers();

            var p1 = ts.Players.First();
            var p2 = ts.Players.Skip(1).First();
            ts.TryPlaceShip(p1, ts.ShipsToPlace(p1).First(), new Position(4, 4));
            ts.TryPlaceShip(p1, ts.ShipsToPlace(p1).First(), new Position(0, 0));
            ts.FinishPlacingShips(p1);
            ts.TryPlaceShip(p2, ts.ShipsToPlace(p2).First(), new Position(4, 4));
            ts.TryPlaceShip(p2, ts.ShipsToPlace(p2).First(), new Position(0, 0));
            ts.FinishPlacingShips(p2);

            ts.TryHit(p1, new Position(4, 4), out _);
            ts.TryHit(p2, new Position(1, 0), out _);

            // Act
            ts.HitSuggested(p1);
            ts.TryHit(p2, new Position(2, 0), out _);
            ts.HitSuggested(p1);
            ts.TryHit(p2, new Position(3, 0), out _);
            ts.HitSuggested(p1);
            ts.TryHit(p2, new Position(4, 0), out _);
            ts.HitSuggested(p1);
            ts.TryHit(p2, new Position(5, 0), out _);

            // Assert
            ts.HitResults.TryGetValue(p1, out var hitResults);
            var hitPositions = hitResults.Keys;
            Assert.IsFalse(hitPositions.Except(new[]
            {
                new Position(4, 4),
                new Position(3, 4),
                new Position(5, 4),
                new Position(4, 3),
                new Position(4, 5),
            }).Any());
        }
    }
}