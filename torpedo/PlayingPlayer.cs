using System;

namespace NationalInstruments
{
    internal abstract class Playable
    {
        protected readonly Player _player;
        protected readonly TorpedoService _torpedoService;

        public Playable(Player player, TorpedoService torpedoService)
        {
            this._player = player;
            this._torpedoService = torpedoService;
        }

        public abstract void PlaceShip(Ship ship);
        public abstract void Hit();
    }

    internal class AIPlayablePlayer : Playable
    {
        public AIPlayablePlayer(Player player, TorpedoService torpedoService) : base(player, torpedoService)
        {
        }

        public override void Hit()
        {
            _torpedoService.HitSuggested(_player);
        }

        public override void PlaceShip(Ship ship)
        {
            _torpedoService.PlaceShipRandom(_player, ship);
        }
    }
    internal class UserPlayablePlayer : Playable
    {
        public UserPlayablePlayer(Player player, TorpedoService torpedoService) : base(player, torpedoService)
        {
        }

        public Action? HitAction { get; set; }
        public Action<Ship>? PlaceAction { get; set; }

        public override void Hit()
        {
            if (HitAction is not null)
            {
                HitAction();
            }
        }

        public override void PlaceShip(Ship ship)
        {
            if (PlaceAction is not null)
            {
                PlaceAction(ship);
            }
        }
    }
}
