using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cardpool
{
    public record Card(int Id, string Name, int Priority, int TimeNeeded, List<string> Tags);

    class Program
    {
        static List<Card> pile = new List<Card>();
        static string filePath = "cards.txt";
        static string logFilePath = "session_logs.txt";
        static Random rnd = new Random();

        static void Main()
        {
            LoadPile();
            string[] options = { "Start Study/WarmUp Session", "Add Exercise/WarmUp Card", "List Cards", "Modify/Delete Card", "Reshuffle Pile", "View Session Logs", "Help", "Exit" };
            int selectedIndex = 0;

            while (true)
            {
                Console.Clear();
                DrawHeader();

                for (int i = 0; i < options.Length; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.Cyan;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.WriteLine($"  ► {options[i].PadRight(36)} ");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine($"    {options[i].PadRight(36)} ");
                        Console.ResetColor();
                    }
                }

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("\n  Use ↑/↓ arrows to navigate and Enter to select.");
                Console.ResetColor();

                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.UpArrow)
                {
                    selectedIndex = (selectedIndex == 0) ? options.Length - 1 : selectedIndex - 1;
                }
                else if (key == ConsoleKey.DownArrow)
                {
                    selectedIndex = (selectedIndex == options.Length - 1) ? 0 : selectedIndex + 1;
                }
                else if (key == ConsoleKey.Enter)
                {
                    Console.Clear();
                    DrawHeader();
                    switch (selectedIndex)
                    {
                        case 0:
                            StartSession();
                            Console.WriteLine("\nPress any key to return...");
                            Console.ReadKey(true);
                            break;
                        case 1:
                            AddCard();
                            Console.WriteLine("\nPress any key to return...");
                            Console.ReadKey(true);
                            break;
                        case 2:
                            ListCards();
                            Console.WriteLine("\nPress any key to return...");
                            Console.ReadKey(true);
                            break;
                        case 3:
                            ModifyOrDeleteCard();
                            Console.WriteLine("\nPress any key to return...");
                            Console.ReadKey(true);
                            break;
                        case 4:
                            ReshufflePileInteractive();
                            Console.WriteLine("\nPress any key to return...");
                            Console.ReadKey(true);
                            break;
                        case 5:
                            ViewLogs();
                            Console.WriteLine("\nPress any key to return...");
                            Console.ReadKey(true);
                            break;
                        case 6:
                            ShowHelp();
                            Console.WriteLine("\nPress any key to return...");
                            Console.ReadKey(true);
                            break;
                        case 7:
                            return;
                    }
                }
            }
        }

        static void DrawHeader()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("  ╔════════════════════════════════════════╗");
            Console.WriteLine("  ║       CARDPOOL SPACED REPETITION       ║");
            Console.WriteLine("  ╚════════════════════════════════════════╝\n");
            Console.ResetColor();
        }

        static void AddCard()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  [ Add New Exercise ]");
            Console.ResetColor();
            
            Console.Write("  Exercise Name: ");
            string name = Console.ReadLine() ?? "Unknown";

            Console.Write("  Priority (e.g., 1 to 5): ");
            if (!int.TryParse(Console.ReadLine(), out int priority) || priority < 1) priority = 1;

            Console.Write("  Time Needed (minutes): ");
            if (!int.TryParse(Console.ReadLine(), out int time) || time < 1) time = 1;

            Console.Write("  Tags (comma separated, optional): ");
            string tagsInput = Console.ReadLine() ?? "";
            var tagsList = tagsInput.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim().ToLower()).ToList();

            int newId = pile.Count > 0 ? pile.Max(c => c.Id) + 1 : 1;
            Card newCard = new Card(newId, name, priority, time, tagsList);

            // A card of n priority appears n times
            for (int i = 0; i < priority; i++)
            {
                pile.Add(newCard);
            }

            ReshufflePile();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n  ✓ Added '{name}' {priority} times to the pile.");
            Console.ResetColor();
        }

        static void StartSession()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  [ Start Study Session ]");
            Console.ResetColor();

            if (pile.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  The pile is empty. Add some cards first.");
                Console.ResetColor();
                return;
            }

            var allTags = pile.SelectMany(c => c.Tags).Distinct().OrderBy(t => t).ToList();
            List<string> selectedTags = new List<string>();

            if (allTags.Any())
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"\n  Available Tags: {string.Join(", ", allTags)}");
                Console.ResetColor();
                Console.Write("  Filter by tags (comma separated, leave blank for all): ");
                string tagInput = Console.ReadLine() ?? "";
                selectedTags = tagInput.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                       .Select(t => t.Trim().ToLower()).ToList();
            }

            Console.Write("  How much time do you want to study (minutes)? ");
            if (!int.TryParse(Console.ReadLine(), out int studyTime) || studyTime <= 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  Invalid time entered.");
                Console.ResetColor();
                return;
            }

            List<Card> sessionCards = new List<Card>();
            HashSet<int> seenIds = new HashSet<int>();
            int timeRemaining = studyTime;

            // Select from the top of the pile cards that fit in the time
            for (int i = 0; i < pile.Count; i++)
            {
                bool matchesTags = selectedTags.Count == 0 || pile[i].Tags.Any(t => selectedTags.Contains(t));

                if (matchesTags && !seenIds.Contains(pile[i].Id) && pile[i].TimeNeeded <= timeRemaining)
                {
                    sessionCards.Add(pile[i]);
                    seenIds.Add(pile[i].Id);
                    timeRemaining -= pile[i].TimeNeeded;
                    pile.RemoveAt(i);
                    i--; // Adjust index backwards after removal to check the new item at this index
                }
            }

            if (sessionCards.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n  No cards found that fit within the remaining study time.");
                Console.ResetColor();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n  --- Session Plan ---");
            Console.ResetColor();
            foreach (var card in sessionCards)
            {
                Console.WriteLine($"    - {card.Name} ({card.TimeNeeded} min)");
            }
            Console.ForegroundColor = ConsoleColor.Green;
            int matchedTime = studyTime - timeRemaining;
            Console.WriteLine($"\n  Total session time matched: {matchedTime} min. Unused time: {timeRemaining} min.");
            Console.ResetColor();

            Console.Write("\n  Do you want to start the session now? (Y/N): ");
            var startKey = Console.ReadKey(true).Key;
            if (startKey == ConsoleKey.N)
            {
                Console.WriteLine("N");
                // Put them back to the top
                pile.InsertRange(0, sessionCards);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  Session cancelled.");
                Console.ResetColor();
                return;
            }
            Console.WriteLine("Y\n");

            if (matchedTime > 0)
            {
                int stretches = RunSessionTimer(matchedTime * 60, sessionCards);
                LogSession(matchedTime, stretches, sessionCards);
            }

            // Move the cards used in the session to the bottom of the pile
            pile.AddRange(sessionCards);
            SavePile();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n  ✓ Session Complete! Great job.");
            Console.ResetColor();
        }

        static int RunSessionTimer(int totalSeconds, List<Card> cards)
        {
            int stretchCount = 0;

            // Calculate breakpoints based on cumulative minutes of cards
            List<double> checkpoints = new List<double>();
            if (cards != null && cards.Count > 0 && totalSeconds > 0)
            {
                int cumulativeMinutes = 0;
                // Exclude the very last card from being a checkpoint since it's the end of the bar
                for (int i = 0; i < cards.Count - 1; i++)
                {
                    cumulativeMinutes += cards[i].TimeNeeded;
                    double fraction = (double)(cumulativeMinutes * 60) / totalSeconds;
                    checkpoints.Add(fraction);
                }
            }

            while (true)
            {
                try { Console.CursorVisible = false; } catch { }

                for (int i = totalSeconds; i >= 0; i--)
                {
                    int barLength = 40;
                    int elapsed = totalSeconds - i;
                    double progress = totalSeconds > 0 ? (double)elapsed / totalSeconds : 1;
                    int filled = (int)(progress * barLength);

                    char[] barChars = new char[barLength];
                    for (int j = 0; j < barLength; j++)
                    {
                        barChars[j] = j < filled ? '█' : '░';
                    }

                    // Overlay checkpoints if in initial session
                    if (stretchCount == 0)
                    {
                        foreach (var cp in checkpoints)
                        {
                            int cpIndex = (int)(cp * barLength);
                            if (cpIndex >= 0 && cpIndex < barLength)
                            {
                                // Show filled marker if elapsed past the checkpoint, otherwise empty marker
                                barChars[cpIndex] = cpIndex < filled ? '╬' : '│';
                            }
                        }
                    }

                    string bar = new string(barChars);
                    TimeSpan time = TimeSpan.FromSeconds(i);

                    Console.Write($"\r  [{bar}] 🏁 {time:hh\\:mm\\:ss} left   ");

                    if (i > 0)
                    {
                        System.Threading.Thread.Sleep(1000);
                    }
                }

                try { Console.CursorVisible = true; } catch { }

                for (int b = 0; b < 3; b++)
                {
                    Console.Beep(800, 300);
                    System.Threading.Thread.Sleep(100);
                }

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n\n  Time's up!");
                Console.ResetColor();

                Console.Write("  Do you want to stretch the session 5 more minutes? (Y/N): ");
                var stretchKey = Console.ReadKey(true).Key;
                if (stretchKey == ConsoleKey.Y)
                {
                    Console.WriteLine("Y\n");
                    totalSeconds = 5 * 60;
                    stretchCount++;
                }
                else
                {
                    Console.WriteLine("N");
                    break;
                }
            }
            return stretchCount;
        }

        static void LogSession(int initialTime, int stretchCount, List<Card> cards)
        {
            try
            {
                int totalTimeSpent = initialTime + (stretchCount * 5);
                string cardsUsed = string.Join(", ", cards.Select(c => c.Name));
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Time Spent: {totalTimeSpent} min (Base: {initialTime} min, Stretches: {stretchCount}) | Cards: {cardsUsed}";
                File.AppendAllLines(logFilePath, new[] { logEntry });
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n  Error saving log: {ex.Message}");
                Console.ResetColor();
            }
        }

        static void ViewLogs()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  [ Session Logs ]\n");
            Console.ResetColor();

            if (!File.Exists(logFilePath))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  No logs found. Complete a session first.");
                Console.ResetColor();
                return;
            }

            try
            {
                var lines = File.ReadAllLines(logFilePath);
                if (lines.Length == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("  No logs found.");
                    Console.ResetColor();
                    return;
                }

                int todayScore = 0;
                int weekScore = 0;
                int monthScore = 0;

                DateTime now = DateTime.Now;
                DateTime today = now.Date;
                DateTime startOfWeek = today.AddDays(-(int)now.DayOfWeek);
                DateTime startOfMonth = new DateTime(now.Year, now.Month, 1);

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    try
                    {
                        int endBracket = line.IndexOf(']');
                        if (endBracket > 1)
                        {
                            string datePart = line.Substring(1, endBracket - 1);
                            if (DateTime.TryParse(datePart, out DateTime logDate))
                            {
                                int baseIdx = line.IndexOf("Base: ");
                                int minIdx = line.IndexOf(" min,", baseIdx);
                                if (baseIdx >= 0 && minIdx > baseIdx)
                                {
                                    string baseStr = line.Substring(baseIdx + 6, minIdx - baseIdx - 6);
                                    if (int.TryParse(baseStr, out int baseTime))
                                    {
                                        if (logDate.Date == today) todayScore += baseTime;
                                        if (logDate.Date >= startOfWeek) weekScore += baseTime;
                                        if (logDate.Date >= startOfMonth) monthScore += baseTime;
                                    }
                                }
                            }
                        }
                    }
                    catch { }
                }

                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("  --- Performance Stats (Base Time: 1 min = 1 pt) ---");
                Console.ResetColor();
                Console.WriteLine($"  Today's Score:      {todayScore} pts");
                Console.WriteLine($"  This Week's Score:  {weekScore} pts");
                Console.WriteLine($"  This Month's Score: {monthScore} pts\n");

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  --- Log History ---");
                Console.ResetColor();

                foreach (var line in lines)
                {
                    Console.WriteLine($"  {line}");
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  Error reading logs: {ex.Message}");
                Console.ResetColor();
            }
        }

        static void ShowHelp()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  [ Help - How to use Cardpool ]\n");
            Console.ResetColor();

            Console.WriteLine("  Cardpool is a spaced repetition application designed to help you study or warm up effectively.\n");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  Core Concepts:");
            Console.ResetColor();
            Console.WriteLine("  - Cards: Each card represents an exercise or topic.");
            Console.WriteLine("  - Tags: Categories to filter cards for a study session.");
            Console.WriteLine("  - Priority: Higher priority (e.g., 5) means the card appears more frequently in the pile.");
            Console.WriteLine("  - Time Needed: The duration (in minutes) required to complete the card's exercise.");
            Console.WriteLine("  - The Pile: Cards are distributed throughout the sequence based on their priority.");
            Console.WriteLine("              When you start a session, cards are pulled from the top of the pile.");
            Console.WriteLine("              After the session, those cards are moved to the bottom.\n");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  Features:");
            Console.ResetColor();
            Console.WriteLine("  1. Start Session: Filter by tags and enter available time. The timer progress bar");
            Console.WriteLine("                    shows checkpoints ('╬') marking the transitions between exercises.");
            Console.WriteLine("  2. Add Card: Create an exercise and assign tags. Added multiple times based on priority.");
            Console.WriteLine("  3. List Cards: View a table of all unique cards currently in your pool.");
            Console.WriteLine("  4. Modify/Delete: Edit a card's details (including tags) or remove it completely.");
            Console.WriteLine("  5. Reshuffle: Randomize the pile to keep same-type tasks from appearing back-to-back.");
            Console.WriteLine("  6. Logs: View daily, weekly, and monthly performance scores (1 base minute = 1 pt).");
        }

        static void ListCards()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  [ Saved Cards ]");
            Console.ResetColor();

            if (pile.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  No cards saved.");
                Console.ResetColor();
                return;
            }

            var uniqueCards = pile.DistinctBy(c => c.Id).OrderBy(c => c.Id).ToList();

            Console.WriteLine("  ┌────┬────────────────────────────────────┬──────────┬─────────────┬────────────────────────┐");
            Console.WriteLine("  │ ID │ Name                               │ Priority │ Time (mins) │ Tags                   │");
            Console.WriteLine("  ├────┼────────────────────────────────────┼──────────┼─────────────┼────────────────────────┤");
            foreach (var card in uniqueCards)
            {
                string tagsStr = string.Join(",", card.Tags);
                if (tagsStr.Length > 22) tagsStr = tagsStr.Substring(0, 19) + "...";
                Console.WriteLine($"  │ {card.Id,2} │ {card.Name,-34} │ {card.Priority,8} │ {card.TimeNeeded,11} │ {tagsStr,-22} │");
            }
            Console.WriteLine("  └────┴────────────────────────────────────┴──────────┴─────────────┴────────────────────────┘");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n  Total unique cards: {uniqueCards.Count} (Total in pool: {pile.Count})");
            Console.ResetColor();
        }

        static void ModifyOrDeleteCard()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  [ Modify/Delete Card ]");
            Console.ResetColor();

            if (pile.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  No cards to modify or delete.");
                Console.ResetColor();
                return;
            }

            ListCards();
            Console.WriteLine();

            Console.Write("  Enter the ID of the card you want to modify or delete: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  Invalid ID.");
                Console.ResetColor();
                return;
            }

            var cardInstances = pile.Where(c => c.Id == id).ToList();
            if (cardInstances.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  No card found with ID {id}.");
                Console.ResetColor();
                return;
            }

            var cardToEdit = cardInstances.First();
            Console.WriteLine($"\n  Selected Card: {cardToEdit.Name} (Priority: {cardToEdit.Priority}, Time: {cardToEdit.TimeNeeded} min)");

            Console.WriteLine("\n  1. Modify Card");
            Console.WriteLine("  2. Delete Card");
            Console.Write("  Choose an option: ");

            var choice = Console.ReadLine();
            if (choice == "1")
            {
                Console.Write($"  New Name (leave blank to keep '{cardToEdit.Name}'): ");
                string newName = Console.ReadLine() ?? "";
                if (string.IsNullOrWhiteSpace(newName)) newName = cardToEdit.Name;

                Console.Write($"  New Priority (leave blank to keep {cardToEdit.Priority}): ");
                string priorityStr = Console.ReadLine() ?? "";
                int newPriority = int.TryParse(priorityStr, out int p) && p >= 1 ? p : cardToEdit.Priority;

                Console.Write($"  New Time Needed (leave blank to keep {cardToEdit.TimeNeeded} mins): ");
                string timeStr = Console.ReadLine() ?? "";
                int newTime = int.TryParse(timeStr, out int t) && t >= 1 ? t : cardToEdit.TimeNeeded;

                Console.Write($"  New Tags (leave blank to keep '{string.Join(",", cardToEdit.Tags)}'): ");
                string newTagsStr = Console.ReadLine() ?? "";
                List<string> newTags = string.IsNullOrWhiteSpace(newTagsStr) 
                    ? cardToEdit.Tags 
                    : newTagsStr.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(tag => tag.Trim().ToLower()).ToList();

                // Remove all old instances with this ID
                pile.RemoveAll(c => c.Id == id);

                // Add new instances based on updated priority
                Card updatedCard = new Card(id, newName, newPriority, newTime, newTags);
                for (int i = 0; i < newPriority; i++)
                {
                    pile.Add(updatedCard);
                }

                ReshufflePile();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n  ✓ Card ID {id} has been modified successfully.");
                Console.ResetColor();
            }
            else if (choice == "2")
            {
                pile.RemoveAll(c => c.Id == id);
                SavePile();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n  ✓ Card ID {id} has been deleted.");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  Action cancelled.");
                Console.ResetColor();
            }
        }

        static void ReshufflePileInteractive()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  [ Reshuffle Pile ]");
            Console.ResetColor();

            if (pile.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  The pile is empty. Add some cards first.");
                Console.ResetColor();
                return;
            }

            ReshufflePile();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n  ✓ Pile has been reshuffled with spacing bias.");
            Console.ResetColor();
        }

        static void ReshufflePile()
        {
            if (pile.Count <= 1) return;

            var newPile = new List<Card>();
            var groups = pile.GroupBy(c => c.Id)
                             .ToDictionary(g => g.Key, g => g.Count());

            var availableCards = pile.DistinctBy(c => c.Id).ToDictionary(c => c.Id, c => c);

            int lastId = -1;

            while (groups.Values.Sum() > 0)
            {
                var candidates = groups.Where(g => g.Value > 0 && g.Key != lastId).ToList();

                if (candidates.Count == 0)
                {
                    // Have to fall back to the same ID if no other choice exists
                    candidates = groups.Where(g => g.Value > 0).ToList();
                }

                // Bias towards groups that have more instances remaining to spread them out
                int maxCount = candidates.Max(c => c.Value);
                var topCandidates = candidates.Where(c => c.Value == maxCount).ToList();

                int chosenId = topCandidates[rnd.Next(topCandidates.Count)].Key;

                newPile.Add(availableCards[chosenId]);
                groups[chosenId]--;
                lastId = chosenId;
            }

            pile = newPile;
            SavePile();
        }

        static void LoadPile()
        {
            if (!File.Exists(filePath)) return;

            try
            {
                var lines = File.ReadAllLines(filePath);
                Dictionary<string, int> legacyIds = new Dictionary<string, int>();
                int nextId = 1;

                foreach (var line in lines)
                {
                    var parts = line.Split('|');
                    if (parts.Length >= 4 && int.TryParse(parts[0], out int id) && int.TryParse(parts[2], out int p) && int.TryParse(parts[3], out int t))
                    {
                        var tags = parts.Length >= 5 ? parts[4].Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>();
                        pile.Add(new Card(id, parts[1], p, t, tags));
                        if (id >= nextId) nextId = id + 1;
                    }
                    else if (parts.Length >= 3 && int.TryParse(parts[1], out int pOld) && int.TryParse(parts[2], out int tOld))
                    {
                        if (!legacyIds.TryGetValue(parts[0], out int legacyId))
                        {
                            legacyId = nextId++;
                            legacyIds[parts[0]] = legacyId;
                        }
                        pile.Add(new Card(legacyId, parts[0], pOld, tOld, new List<string>()));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error loading cards: {ex.Message}");
                Console.ResetColor();
            }
        }

        static void SavePile()
        {
            try
            {
                File.WriteAllLines(filePath, pile.Select(c => $"{c.Id}|{c.Name}|{c.Priority}|{c.TimeNeeded}|{string.Join(",", c.Tags)}"));
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error saving cards: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}