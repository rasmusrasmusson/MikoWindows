
using MikoMe.Models;
using System;

namespace MikoMe.Services;

public static class Scheduler
{
    public static void ApplyReview(Card card, bool known, DateTime nowUtc)
    {
        int q = known ? 4 : 2;
        double ef = card.Ease <= 0 ? 2.5 : card.Ease;

        if (q < 3)
        {
            card.Reps = 0;
            card.IntervalDays = 1;
            card.Lapses += 1;
            card.State = "Learning";
        }
        else
        {
            card.Reps += 1;
            ef = ef + (0.1 - (5 - q) * (0.08 + (5 - q) * 0.02));
            if (ef < 1.3) ef = 1.3;
            if (ef > 2.8) ef = 2.8;

            if (card.Reps == 1) card.IntervalDays = 1;
            else if (card.Reps == 2) card.IntervalDays = 6;
            else card.IntervalDays = (int)Math.Round(card.IntervalDays * ef);

            card.State = "Review";
        }

        card.Ease = ef;
        card.LastReviewedAtUtc = nowUtc;
        card.DueAtUtc = nowUtc.AddDays(card.IntervalDays);
    }
}
