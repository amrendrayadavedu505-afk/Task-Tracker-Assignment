using Task_Tracker.Domain;
using Task_Tracker.Application;

Console.WriteLine("=== TASK TRACKER ===");
Console.WriteLine("1) Add Task");
Console.WriteLine("2) Update Task Status");
Console.WriteLine("3) Search Tasks");
Console.WriteLine("4) Show Overdue Tasks");
Console.WriteLine("5) Export Overdue to CSV");
Console.WriteLine("6) List Tasks (sorted by Due Date)");   // added
Console.WriteLine("7) List Tasks (sorted by Priority)");   // added
Console.WriteLine("0) Exit");

var tasks = SimpleStorage.Load();
SimpleLog.Info("App started. Loaded tasks: " + tasks.Count);

while (true)
{
    Console.Write("\nChoose: ");
    var menuChoice = (Console.ReadLine() ?? "").Trim();

    if (menuChoice == "0")
    {
        SimpleLog.Info("App closed by user.");
        break;
    }

    // ------------------ ADD TASK ------------------
    if (menuChoice == "1")
    {
        try
        {
            var item = new TaskItem();

            // TITLE (required)
            while (true)
            {
                Console.Write("Title (required): ");
                var titleInput = Console.ReadLine() ?? "";
                if (!string.IsNullOrWhiteSpace(titleInput))
                {
                    item.Title = titleInput.Trim();
                    break;
                }
                Console.WriteLine("Please enter a title.");
            }

            // DESCRIPTION
            Console.Write("Description: ");
            item.Description = (Console.ReadLine() ?? "").Trim();

            // ASSIGNEE (required)
            while (true)
            {
                Console.Write("Assignee (name or email, required): ");
                var assigneeInput = (Console.ReadLine() ?? "").Trim();
                if (!string.IsNullOrWhiteSpace(assigneeInput))
                {
                    item.Assignee = assigneeInput;
                    break;
                }
                Console.WriteLine("Assignee cannot be empty.");
            }

            // START DATE (blank = today; cannot be in the past)
            while (true)
            {
                Console.Write("Start date (yyyy-MM-dd): ");
                var startInput = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(startInput))
                {
                    item.StartDate = DateTime.UtcNow.Date;
                    break;
                }

                if (DateTime.TryParse(startInput, out var startDate))
                {
                    startDate = startDate.Date;
                    if (startDate < DateTime.UtcNow.Date)
                    {
                        Console.WriteLine("Start date cannot be in the past.");
                    }
                    else
                    {
                        item.StartDate = startDate;
                        break;
                    }
                }
                else
                {
                    Console.WriteLine("Please use the format yyyy-MM-dd.");
                }
            }

            // DUE DATE (blank = start + 7 days; must be >= start)
            while (true)
            {
                Console.Write("Due date (yyyy-MM-dd): ");
                var dueInput = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(dueInput))
                {
                    item.DueDate = item.StartDate.AddDays(7);
                    break;
                }

                if (DateTime.TryParse(dueInput, out var dueDate))
                {
                    dueDate = dueDate.Date;
                    if (dueDate < item.StartDate)
                    {
                        Console.WriteLine("Due date cannot be before the start date.");
                    }
                    else
                    {
                        item.DueDate = dueDate;
                        break;
                    }
                }
                else
                {
                    Console.WriteLine("Please use the format yyyy-MM-dd.");
                }
            }

            // PRIORITY (defaults to Medium if invalid)
            Console.Write("Priority (Low/Medium/High/Critical): ");
            var priorityInput = (Console.ReadLine() ?? "").Trim();
            if (!Enum.TryParse<Priority>(priorityInput, true, out var parsedPriority)) parsedPriority = Priority.Medium;
            item.Priority = parsedPriority;

            item.Status = Status.Todo;

            // save
            tasks.Add(item);
            SimpleStorage.Save(tasks);

            Console.WriteLine("\nSaved");
            Console.WriteLine($"ID: {item.Id}");
            Console.WriteLine($"Title: {item.Title}");
            Console.WriteLine($"Assignee: {item.Assignee}");
            Console.WriteLine($"Start: {item.StartDate:yyyy-MM-dd}");
            Console.WriteLine($"Due:   {item.DueDate:yyyy-MM-dd}");
            Console.WriteLine($"Priority: {item.Priority}");
            Console.WriteLine($"Status: {item.Status}");

            SimpleLog.Info($"Task added: {item.Title} ({item.Id.Substring(0,6)}) assignee={item.Assignee} start={item.StartDate:yyyy-MM-dd} due={item.DueDate:yyyy-MM-dd} prio={item.Priority}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to add task.");
            SimpleLog.Error("Add Task failed: " + ex.Message);
        }
        continue;
    }

    // ------------------ UPDATE TASK STATUS ------------------
    if (menuChoice == "2")
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
            Console.WriteLine($"[{i + 1}] {x.Id.Substring(0, 6)} | {x.Title} | {x.Status} | {x.StartDate:yyyy-MM-dd} -> {x.DueDate:yyyy-MM-dd}");
        }

        Console.WriteLine("\nSelect a task by Number, ID (first 6 or full), or Title");
        Console.Write("Your input: ");
        var pickInput = (Console.ReadLine() ?? "").Trim();

        TaskItem? target = null;

        // number
        if (int.TryParse(pickInput, out var indexNumber))
            if (indexNumber >= 1 && indexNumber <= tasks.Count) target = tasks[indexNumber - 1];

        // id
        if (target == null)
        {
            if (pickInput.Length <= 6)
                target = tasks.Find(t => t.Id.StartsWith(pickInput, StringComparison.OrdinalIgnoreCase));
            if (target == null)
                target = tasks.Find(t => string.Equals(t.Id, pickInput, StringComparison.OrdinalIgnoreCase));
        }

        // title
        if (target == null)
        {
            target = tasks.Find(t => string.Equals(t.Title, pickInput, StringComparison.OrdinalIgnoreCase));
            if (target == null)
                target = tasks.Find(t => t.Title.IndexOf(pickInput, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        if (target == null)
        {
            Console.WriteLine("Task not found.");
            SimpleLog.Info("Update status: task not found for input '" + pickInput + "'");
            continue;
        }

        Console.WriteLine($"\nSelected: {target.Title} ({target.Id.Substring(0,6)})");
        Console.WriteLine($"Current status: {target.Status}");
        Console.WriteLine("New status options: Todo, InProgress, Done");

        Status newStatus;
        while (true)
        {
            Console.Write("Set new status: ");
            var statusInput = (Console.ReadLine() ?? "").Trim();
            if (Enum.TryParse<Status>(statusInput, true, out newStatus))
                break;
            Console.WriteLine("Invalid status. Use: Todo, InProgress, Done");
        }

        var old = target.Status;
        target.Status = newStatus;
        SimpleStorage.Save(tasks);

        Console.WriteLine("Status updated.");
        SimpleLog.Info($"Status change: {target.Title} ({target.Id.Substring(0,6)}) {old} -> {newStatus}");
        continue;
    }

    // ------------------ SEARCH TASKS ------------------
    if (menuChoice == "3")
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
        var searchChoice = (Console.ReadLine() ?? "").Trim();

        try
        {
            SimpleLog.Info("Search started: option=" + searchChoice);
            List<TaskItem> found = new List<TaskItem>();

            if (searchChoice == "1")
            {
                Console.Write("Enter title text: ");
                var query = Console.ReadLine() ?? "";
                found = SimpleSearch.ByTitle(tasks, query);
            }
            else if (searchChoice == "2")
            {
                Console.Write("Enter ID or first 6 chars: ");
                var idQuery = Console.ReadLine() ?? "";
                found = SimpleSearch.ByIdPrefix(tasks, idQuery);
            }
            else if (searchChoice == "3")
            {
                Console.Write("Enter assignee text: ");
                var whoQuery = Console.ReadLine() ?? "";
                found = SimpleSearch.ByAssignee(tasks, whoQuery);
            }
            else if (searchChoice == "4")
            {
                Console.WriteLine("1) On date   2) Before date   3) After date");
                Console.Write("Choose: ");
                var whenChoice = (Console.ReadLine() ?? "").Trim();

                Console.Write("Enter date (yyyy-MM-dd): ");
                var dateText = (Console.ReadLine() ?? "").Trim();
                if (!DateTime.TryParse(dateText, out var dateQuery))
                {
                    Console.WriteLine("Bad date format.");
                    SimpleLog.Error("Search date parse failed: '" + dateText + "'");
                    continue;
                }

                if (whenChoice == "1") found = SimpleSearch.ByDueOn(tasks, dateQuery);
                else if (whenChoice == "2") found = SimpleSearch.ByDueBefore(tasks, dateQuery);
                else if (whenChoice == "3") found = SimpleSearch.ByDueAfter(tasks, dateQuery);
                else { Console.WriteLine("Unknown option."); continue; }
            }
            else if (searchChoice == "5")
            {
                Console.Write("Priority (Low/Medium/High/Critical): ");
                var prioText = (Console.ReadLine() ?? "").Trim();
                if (!Enum.TryParse<Priority>(prioText, true, out var prioEnum))
                {
                    Console.WriteLine("Invalid priority.");
                    SimpleLog.Error("Search priority parse failed: '" + prioText + "'");
                    continue;
                }
                found = SimpleSearch.ByPriority(tasks, prioEnum);
            }
            else
            {
                Console.WriteLine("Unknown search option.");
                continue;
            }

            Console.WriteLine(found.Count == 0 ? "No results." : $"\nFound {found.Count}:");
            for (int i = 0; i < found.Count; i++)
            {
                var x = found[i];
                Console.WriteLine($"{x.Id.Substring(0,6)} | {x.Title} | {x.Assignee} | {x.Priority} | {x.Status} | {x.StartDate:yyyy-MM-dd} -> {x.DueDate:yyyy-MM-dd}");
            }

            SimpleLog.Info("Search finished. results=" + found.Count);
        }
        catch (Exception ex)
        {
            SimpleLog.Error("Search failed: " + ex.Message);
        }
        continue;
    }

    // ------------------ SHOW OVERDUE TASKS ------------------
    if (menuChoice == "4")
    {
        if (tasks.Count == 0)
        {
            Console.WriteLine("No tasks yet.");
            continue;
        }

        var overdue = SimpleSearch.Overdue(tasks);
        if (overdue.Count == 0)
        {
            Console.WriteLine("No overdue tasks.");
            SimpleLog.Info("Overdue viewed: none");
            continue;
        }

        Console.WriteLine("\nOverdue tasks:");
        for (int i = 0; i < overdue.Count; i++)
        {
            var x = overdue[i];
            Console.WriteLine($"{x.Id.Substring(0,6)} | {x.Title} | {x.Assignee} | {x.Status} | Due {x.DueDate:yyyy-MM-dd}");
        }
        SimpleLog.Info("Overdue viewed: count=" + overdue.Count);
        continue;
    }

    // ------------------ EXPORT OVERDUE TO CSV ------------------
    if (menuChoice == "5")
    {
        if (tasks.Count == 0)
        {
            Console.WriteLine("No tasks yet.");
            continue;
        }

        var overdue = SimpleSearch.Overdue(tasks);
        if (overdue.Count == 0)
        {
            Console.WriteLine("No overdue tasks to export.");
            SimpleLog.Info("Export skipped: no overdue tasks.");
            continue;
        }

        try
        {
            var path = CsvExport.OverdueToCsv(overdue);
            Console.WriteLine($"Exported to: {path}");
            SimpleLog.Info("Exported overdue CSV: " + path);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Export failed.");
            SimpleLog.Error("Export failed: " + ex.Message);
        }
        continue;
    }

    // ------------------ LIST (sorted by Due Date) ------------------
    if (menuChoice == "6")
    {
        if (tasks.Count == 0)
        {
            Console.WriteLine("No tasks to list.");
            continue;
        }

        var sorted = SimpleSort.ByDueDateAscending(tasks);
        Console.WriteLine("\nTasks (earliest due first):");
        for (int i = 0; i < sorted.Count; i++)
        {
            var x = sorted[i];
            Console.WriteLine($"{x.Id.Substring(0,6)} | {x.Title} | {x.Assignee} | {x.Priority} | {x.Status} | {x.StartDate:yyyy-MM-dd} -> {x.DueDate:yyyy-MM-dd}");
        }
        SimpleLog.Info("Listed tasks sorted by due date. count=" + sorted.Count);
        continue;
    }

    // ------------------ LIST (sorted by Priority) ------------------
    if (menuChoice == "7")
    {
        if (tasks.Count == 0)
        {
            Console.WriteLine("No tasks to list.");
            continue;
        }

        var sorted = SimpleSort.ByPriorityDesc(tasks);
        Console.WriteLine("\nTasks (highest priority first):");
        for (int i = 0; i < sorted.Count; i++)
        {
            var x = sorted[i];
            Console.WriteLine($"{x.Id.Substring(0,6)} | {x.Title} | {x.Assignee} | {x.Priority} | {x.Status} | {x.StartDate:yyyy-MM-dd} -> {x.DueDate:yyyy-MM-dd}");
        }
        SimpleLog.Info("Listed tasks sorted by priority. count=" + sorted.Count);
        continue;
    }

    Console.WriteLine("Unknown option. Use 0, 1, 2, 3, 4, 5, 6, or 7.");
}
