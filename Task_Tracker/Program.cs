using Task_Tracker.Domain;
using Task_Tracker.Application;

Console.WriteLine("=== TASK TRACKER ===");
Console.WriteLine("1) Add Task");
Console.WriteLine("2) Update Task Status");
Console.WriteLine("3) Search Tasks");
Console.WriteLine("0) Exit");

var tasks = SimpleStorage.Load();

while (true)
{
    Console.Write("\nChoose: ");
    var choice = (Console.ReadLine() ?? "").Trim();

    if (choice == "0") break;

    if (choice == "1")
    {
        var item = new TaskItem();

        while (true)
        {
            Console.Write("Title (required): ");
            var t = Console.ReadLine() ?? "";
            if (!string.IsNullOrWhiteSpace(t))
            {
                item.Title = t.Trim();
                break;
            }
            Console.WriteLine("Please enter a title.");
        }
        Console.Write("Description: ");
        item.Description = (Console.ReadLine() ?? "").Trim();
        while (true)
        {
            Console.Write("Assignee (name or email, required): ");
            var assignee = (Console.ReadLine() ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(assignee))
            {
                item.Assignee = assignee;
                break;
            }
            Console.WriteLine("Assignee cannot be empty.");
        }
        while (true)
        {
            Console.Write("Due date (yyyy-MM-dd): ");
            var dueStr = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(dueStr))
            {
                item.DueDate = DateTime.UtcNow.AddDays(7);
                break;
            }

            if (DateTime.TryParse(dueStr, out var date))
            {
                if (date.Date < DateTime.UtcNow.Date)
                {
                    Console.WriteLine("Due date cannot be in the past.");
                }
                else
                {
                    item.DueDate = date;
                    break;
                }
            }
            else
            {
                Console.WriteLine("Please use the format yyyy-MM-dd.");
            }
        }
        Console.Write("Priority (Low/Medium/High/Critical): ");
        var priority = (Console.ReadLine() ?? "").Trim();
        if (!Enum.TryParse<Priority>(priority, true, out var pr)) pr = Priority.Medium;
        item.Priority = pr;

        item.Status = Status.Todo;

        tasks.Add(item);
        SimpleStorage.Save(tasks);

        Console.WriteLine("\nSaved");
        Console.WriteLine($"ID: {item.Id}");
        Console.WriteLine($"Title: {item.Title}");
        Console.WriteLine($"Assignee: {item.Assignee}");
        Console.WriteLine($"Due: {item.DueDate:yyyy-MM-dd}");
        Console.WriteLine($"Priority: {item.Priority}");
        Console.WriteLine($"Status: {item.Status}");
        continue;
    }
    if (choice == "2")
    {
        if (tasks.Count == 0)
        {
            Console.WriteLine("No tasks yet. Add one first.");
            continue;
        }

        // numbered list
        Console.WriteLine("\nExisting tasks:");
        for (int i = 0; i < tasks.Count; i++)
        {
            var x = tasks[i];
            Console.WriteLine($"[{i + 1}] {x.Id.Substring(0, 6)} | {x.Title} | {x.Status} | {x.DueDate:yyyy-MM-dd}");
        }

        Console.WriteLine("\nSelect a task by Number, ID (first 6 or full), or Title");
        Console.Write("Your input: ");
        var pick = (Console.ReadLine() ?? "").Trim();

        TaskItem? target = null;
        if (int.TryParse(pick, out var idxNum))
        {
            if (idxNum >= 1 && idxNum <= tasks.Count) target = tasks[idxNum - 1];
        }
        if (target == null)
        {
            if (pick.Length <= 6)
                target = tasks.Find(t => t.Id.StartsWith(pick, StringComparison.OrdinalIgnoreCase));
            if (target == null)
                target = tasks.Find(t => string.Equals(t.Id, pick, StringComparison.OrdinalIgnoreCase));
        }
        if (target == null)
        {
            target = tasks.Find(t => string.Equals(t.Title, pick, StringComparison.OrdinalIgnoreCase));
            if (target == null)
                target = tasks.Find(t => t.Title.IndexOf(pick, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        if (target == null)
        {
            Console.WriteLine("Task not found.");
            continue;
        }

        Console.WriteLine($"\nSelected: {target.Title} ({target.Id.Substring(0,6)})");
        Console.WriteLine($"Current status: {target.Status}");
        Console.WriteLine("New status options: Todo, InProgress, Done");

        Status newStatus;
        while (true)
        {
            Console.Write("Set new status: ");
            var s = (Console.ReadLine() ?? "").Trim();
            if (Enum.TryParse<Status>(s, true, out newStatus)) break;
            Console.WriteLine("Invalid status. Use: Todo, InProgress, Done");
        }

        target.Status = newStatus;
        SimpleStorage.Save(tasks);
        Console.WriteLine("Status updated.");
        continue;
    }
    if (choice == "3")
    {
        if (tasks.Count == 0)
        {
            Console.WriteLine("No tasks to search.");
            continue;
        }

        Console.WriteLine("\nSearch by:");
        Console.WriteLine("1) Title (contains)");
        Console.WriteLine("2) ID prefix (first 6 or full)");
        Console.WriteLine("3) Assignee (contains)");
        Console.WriteLine("4) Due date (On / Before / After)");
        Console.WriteLine("5) Priority (equals)");
        Console.Write("Pick: ");
        var sopt = (Console.ReadLine() ?? "").Trim();

        List<TaskItem> found = new List<TaskItem>();

        if (sopt == "1")
        {
            Console.Write("Enter title text: ");
            var q = Console.ReadLine() ?? "";
            found = SimpleSearch.ByTitle(tasks, q);
        }
        else if (sopt == "2")
        {
            Console.Write("Enter ID or first 6 chars: ");
            var q = Console.ReadLine() ?? "";
            found = SimpleSearch.ByIdPrefix(tasks, q);
        }
        else if (sopt == "3")
        {
            Console.Write("Enter assignee text: ");
            var q = Console.ReadLine() ?? "";
            found = SimpleSearch.ByAssignee(tasks, q);
        }
        else if (sopt == "4")
        {
            Console.WriteLine("1) On date   2) Before date   3) After date");
            Console.Write("Choose: ");
            var m = (Console.ReadLine() ?? "").Trim();

            Console.Write("Enter date (yyyy-MM-dd): ");
            var ds = (Console.ReadLine() ?? "").Trim();
            if (!DateTime.TryParse(ds, out var dt))
            {
                Console.WriteLine("Bad date format.");
                continue;
            }

            if (m == "1") found = SimpleSearch.ByDueOn(tasks, dt);
            else if (m == "2") found = SimpleSearch.ByDueBefore(tasks, dt);
            else if (m == "3") found = SimpleSearch.ByDueAfter(tasks, dt);
            else { Console.WriteLine("Unknown option."); continue; }
        }
        else if (sopt == "5")
        {
            Console.Write("Priority (Low/Medium/High/Critical): ");
            var ptxt = (Console.ReadLine() ?? "").Trim();
            if (!Enum.TryParse<Priority>(ptxt, true, out var prio))
            {
                Console.WriteLine("Invalid priority.");
                continue;
            }
            found = SimpleSearch.ByPriority(tasks, prio);
        }
        else
        {
            Console.WriteLine("Unknown search option.");
            continue;
        }

        if (found.Count == 0)
        {
            Console.WriteLine("No results.");
        }
        else
        {
            Console.WriteLine($"\nFound {found.Count}:");
            for (int i = 0; i < found.Count; i++)
            {
                var x = found[i];
                Console.WriteLine($"{x.Id.Substring(0,6)} | {x.Title} | {x.Assignee} | {x.Priority} | {x.Status} | {x.DueDate:yyyy-MM-dd}");
            }
        }
        continue;
    }

    Console.WriteLine("Unknown option. Use 0, 1, 2, or 3.");
}
