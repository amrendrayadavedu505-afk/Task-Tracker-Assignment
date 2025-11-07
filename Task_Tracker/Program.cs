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
            var task = Console.ReadLine() ?? "";
            if (!string.IsNullOrWhiteSpace(task))
            {
                item.Title = task.Trim();
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
            var status = (Console.ReadLine() ?? "").Trim();
            if (Enum.TryParse<Status>(status, true, out newStatus)) break;
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
        Console.WriteLine("1) Title");
        Console.WriteLine("2) ID prefix");
        Console.WriteLine("3) Assignee");
        Console.Write("Pick: ");
        var sopt = (Console.ReadLine() ?? "").Trim();

        List<TaskItem> found = new List<TaskItem>();

        if (sopt == "1")
        {
            Console.Write("Enter title text: ");
            var title = Console.ReadLine() ?? "";
            found = SimpleSearch.ByTitle(tasks, title);
        }
        else if (sopt == "2")
        {
            Console.Write("Enter ID: ");
            var q = Console.ReadLine() ?? "";
            found = SimpleSearch.ByIdPrefix(tasks, q);
        }
        else if (sopt == "3")
        {
            Console.Write("Enter assignee text: ");
            var assignee = Console.ReadLine() ?? "";
            found = SimpleSearch.ByAssignee(tasks, assignee);
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
                Console.WriteLine($"{x.Id.Substring(0,6)} | {x.Title} | {x.Assignee} | {x.Status} | {x.DueDate:yyyy-MM-dd}");
            }
        }
        continue;
    }

    Console.WriteLine("Unknown option. Use 0, 1, 2, or 3.");
}
