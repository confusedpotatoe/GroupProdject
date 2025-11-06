using BrickBreaker.Game;

var game = new BrickBreakerGame();
int score = game.Run();
Console.WriteLine($"\nFinal score: {score}");
Console.WriteLine("Press any key to exit...");
Console.ReadKey(true); 
