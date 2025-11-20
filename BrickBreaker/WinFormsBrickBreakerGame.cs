using BrickBreaker.Game;
using System.Threading;
using System.Windows.Forms;

namespace BrickBreaker;

/// <summary>
/// Hosts the WinForms gameplay inside an <see cref="IGame"/> abstraction so other
/// application surfaces (console menus, tests, etc.) can launch the desktop game
/// without knowing UI details.
/// </summary>
public sealed class WinFormsBrickBreakerGame : IGame
{
    public int Run()
    {
        using var completion = new ManualResetEventSlim(false);
        Exception? failure = null;
        int finalScore = 0;
        bool scoreCaptured = false;

        var thread = new Thread(() =>
        {
            try
            {
                ApplicationConfiguration.Initialize();
                using var form = new Form1 { CloseOnGameOver = true };

                form.GameFinished += (_, score) =>
                {
                    finalScore = score;
                    scoreCaptured = true;
                    // Closing the form is handled by Form1 when CloseOnGameOver is true.
                };

                form.FormClosed += (_, __) =>
                {
                    if (!scoreCaptured)
                    {
                        finalScore = form.LatestScore;
                    }

                    completion.Set();
                };

                Application.Run(form);
            }
            catch (Exception ex)
            {
                failure = ex;
                completion.Set();
            }
        })
        {
            IsBackground = true
        };

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        completion.Wait();
        thread.Join();

        if (failure is not null)
        {
            throw new InvalidOperationException("Failed to run the WinForms BrickBreaker game.", failure);
        }

        return finalScore;
    }
}
