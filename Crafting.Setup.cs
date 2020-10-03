using BlockID = System.UInt16;

namespace NotAwesomeSurvival {

    public partial class Crafting {

        public static void Setup() {
            Recipe woodPickaxe = new Recipe(new Item("Wood Pickaxe"));
            //woodPickaxe.shapeless = true;
            //woodPickaxe.usesParentID = true;
            //woodPickaxe.stationType = Crafting.Station.Type.Furnace;
            woodPickaxe.pattern = new BlockID[,] {
                {  5,  5, 5 },
                {  0, 78, 0 },
                {  0, 78, 0 }
            };

            // stone tools
            Recipe stonePickaxe = new Recipe(new Item("Stone Pickaxe"));
            stonePickaxe.pattern = new BlockID[,] {
                {  1,  1, 1 },
                {  0, 78, 0 },
                {  0, 78, 0 }
            };
            Recipe stoneShovel = new Recipe(new Item("Stone Shovel"));
            stoneShovel.pattern = new BlockID[,] {
                {   1 },
                {  78 },
                {  78 }
            };
            Recipe stoneAxe = new Recipe(new Item("Stone Axe"));
            stoneAxe.pattern = new BlockID[,] {
                {  1,  1 },
                {  1, 78 },
                {  0, 78 }
            };

            Recipe stoneSword = new Recipe(new Item("Stone Sword"));
            stoneSword.pattern = new BlockID[,] {
                {  1 },
                {  1 },
                { 78 }
            };

            //iron tools
            Recipe ironPickaxe = new Recipe(new Item("Iron Pickaxe"));
            ironPickaxe.pattern = new BlockID[,] {
                { 42, 42,42 },
                {  0, 78, 0 },
                {  0, 78, 0 }
            };
            Recipe ironShovel = new Recipe(new Item("Iron Shovel"));
            ironShovel.pattern = new BlockID[,] {
                {  42 },
                {  78 },
                {  78 }
            };
            Recipe ironAxe = new Recipe(new Item("Iron Axe"));
            ironAxe.pattern = new BlockID[,] {
                { 42, 42 },
                { 42, 78 },
                {  0, 78 }
            };

            Recipe ironSword = new Recipe(new Item("Iron Sword"));
            ironSword.pattern = new BlockID[,] {
                { 42 },
                { 42 },
                { 78 }
            };

            //gold tools
            Recipe goldPickaxe = new Recipe(new Item("Gold Pickaxe"));
            goldPickaxe.pattern = new BlockID[,] {
                { 41, 41,41 },
                {  0, 78, 0 },
                {  0, 78, 0 }
            };
            Recipe goldShovel = new Recipe(new Item("Gold Shovel"));
            goldShovel.pattern = new BlockID[,] {
                {  41 },
                {  78 },
                {  78 }
            };
            Recipe goldAxe = new Recipe(new Item("Gold Axe"));
            goldAxe.pattern = new BlockID[,] {
                { 41, 41 },
                { 41, 78 },
                {  0, 78 }
            };

            Recipe goldSword = new Recipe(new Item("Gold Sword"));
            goldSword.pattern = new BlockID[,] {
                { 41 },
                { 41 },
                { 78 }
            };

            //diamond tools
            Recipe diamondPickaxe = new Recipe(new Item("Diamond Pickaxe"));
            diamondPickaxe.pattern = new BlockID[,] {
                { 631, 631,631 },
                {  0, 78, 0 },
                {  0, 78, 0 }
            };
            Recipe diamondShovel = new Recipe(new Item("Diamond Shovel"));
            diamondShovel.pattern = new BlockID[,] {
                {  631 },
                {  78 },
                {  78 }
            };
            Recipe diamondAxe = new Recipe(new Item("Diamond Axe"));
            diamondAxe.pattern = new BlockID[,] {
                { 631, 631 },
                { 631, 78 },
                {  0, 78 }
            };

            Recipe diamondSword = new Recipe(new Item("Diamond Sword"));
            diamondSword.pattern = new BlockID[,] {
                { 631 },
                { 631 },
                { 78 }
            };


            //wood stuff ------------------------------------------------------
            Recipe wood = new Recipe(5, 4);
            wood.usesParentID = true;
            wood.pattern = new BlockID[,] {
                {  17 }
            };
            Recipe woodSlab = new Recipe(56, 6);
            woodSlab.pattern = new BlockID[,] {
                {  5, 5, 5 }
            };
            Recipe woodWall = new Recipe(182, 6);
            woodWall.pattern = new BlockID[,] {
                {  5 },
                {  5 },
                {  5 }
            };
            Recipe woodStair = new Recipe(66, 6);
            woodStair.pattern = new BlockID[,] {
                {  5, 0, 0 },
                {  5, 5, 0 },
                {  5, 5, 5 }
            };



            Recipe woodPole = new Recipe(78, 4);
            woodPole.pattern = new BlockID[,] {
                {  5 },
                {  5 }
            };

            Recipe fenceWE = new Recipe(94, 4);
            fenceWE.pattern = new BlockID[,] {
                {  78, 79, 78 },
                {  78, 79, 78 }
            };
            Recipe fenceNS = new Recipe(94, 4);
            fenceNS.pattern = new BlockID[,] {
                {  78, 80, 78 },
                {  78, 80, 78 }
            };

            Recipe darkDoor = new Recipe(55, 2);
            darkDoor.pattern = new BlockID[,] {
                { 17, 17 },
                { 17, 17 },
                { 17, 17 }
            };

            //stone stuff ------------------------------------------------------

            Recipe stoneSlab = new Recipe(596, 6);
            stoneSlab.pattern = new BlockID[,] {
                {  1, 1, 1 }
            };
            Recipe stoneWall = new Recipe(598, 6);
            stoneWall.pattern = new BlockID[,] {
                {  1 },
                {  1 },
                {  1 }
            };
            Recipe stoneStair = new Recipe(70, 6);
            stoneStair.pattern = new BlockID[,] {
                {  1, 0, 0 },
                {  1, 1, 0 },
                {  1, 1, 1 }
            };

            Recipe cobbleBrick = new Recipe(4, 4);
            cobbleBrick.pattern = new BlockID[,] {
                {  162, 162 },
                {  162, 162 }
            };
            Recipe cobbleBrickSlab = new Recipe(50, 6);
            cobbleBrickSlab.pattern = new BlockID[,] {
                {  4, 4, 4 }
            };
            //133
            Recipe cobbleBrickWall = new Recipe(133, 6);
            cobbleBrickWall.pattern = new BlockID[,] {
                {  4, 4, 4 },
                {  4, 4, 4 }
            };



            Recipe cobblestone = new Recipe(162, 4);
            cobblestone.pattern = new BlockID[,] {
                {  1, 1 },
                {  1, 1 }
            };
            Recipe cobblestoneSlab = new Recipe(163, 6);
            cobblestoneSlab.pattern = new BlockID[,] {
                {  162, 162, 162 }
            };

            Recipe furnace = new Recipe(625, 1);
            furnace.usesParentID = true;
            furnace.pattern = new BlockID[,] {
                {  1,  1, 1 },
                {  1,  0, 1 },
                {  1,  1, 1 }
            };


            Recipe concreteBlock = new Recipe(45, 4);
            concreteBlock.pattern = new BlockID[,] {
                {  4, 4 },
                {  4, 4 }
            };
            Recipe concreteSlab = new Recipe(44, 6);
            concreteSlab.pattern = new BlockID[,] {
                {  45, 45, 45 }
            };
            Recipe concreteWall = new Recipe(282, 6);
            concreteWall.pattern = new BlockID[,] {
                { 45 },
                { 45 },
                { 45 }
            };
            Recipe concreteBrick = new Recipe(549, 4);
            concreteBrick.pattern = new BlockID[,] {
                {  45, 45 },
                {  45, 45 }
            };
            Recipe stonePlate = new Recipe(135, 6);
            stonePlate.pattern = new BlockID[,] {
                {  44, 44, 44 }
            };
            //upside down slab recipe
            Recipe stonePlate2 = new Recipe(135, 6);
            stonePlate2.pattern = new BlockID[,] {
                {  58, 58, 58 }
            };
            Recipe concreteStair = new Recipe(270, 6);
            concreteStair.pattern = new BlockID[,] {
                {  45,  0,  0 },
                {  45, 45,  0 },
                {  45, 45, 45 }
            };
            Recipe concreteCorner = new Recipe(480, 4);
            concreteCorner.pattern = new BlockID[,] {
                { 45 },
                { 45 }
            };



            //ore stuff
            Recipe coalBlock = new Recipe(49, 1);
            coalBlock.shapeless = true;
            coalBlock.pattern = new BlockID[,] {
                {  627, 627 }
            };
            Recipe hotCoals = new Recipe(239, 1);
            hotCoals.shapeless = true;
            hotCoals.pattern = new BlockID[,] {
                {  49, 49 }
            };


            Recipe iron = new Recipe(42, 1);
            iron.stationType = Crafting.Station.Type.Furnace;
            iron.shapeless = true;
            iron.pattern = new BlockID[,] {
                {  628, 627, 627 },
                {  627, 627, 627 },
                {  627, 627, 627 },
            };
            Recipe gold = new Recipe(41, 1);
            gold.stationType = Crafting.Station.Type.Furnace;
            gold.shapeless = true;
            gold.pattern = new BlockID[,] {
                {  629, 49, 49 },
                {   49, 49, 49 },
                {   49, 49, 49 },
            };
            Recipe diamond = new Recipe(631, 1);
            diamond.stationType = Crafting.Station.Type.Furnace;
            diamond.shapeless = true;
            diamond.pattern = new BlockID[,] {
                {  630, 239, 239 },
                {  239, 239, 239 },
                {  239, 239, 239 },
            };

            Recipe glass = new Recipe(20, 1);
            glass.stationType = Crafting.Station.Type.Furnace;
            glass.shapeless = true;
            glass.pattern = new BlockID[,] {
                { 12 }
            };
        }

    }

}
