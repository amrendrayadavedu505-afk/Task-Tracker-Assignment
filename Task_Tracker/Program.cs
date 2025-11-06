using Task_Tracker.Domain;
using Task_Tracker.Application;

Console.WriteLine("=== TASK TRACKER ===");
Console.WriteLine("1) Add Task");
Console.WriteLine("0) Exit");

var tasks = SimpleStorage.Load();

while (true)
{
    Console.Write("\nChoose: ");
    var choice = Console.ReadLine();

    if (choice == "0") break;

    if (choice == "1")
    {
        var item = new TaskItem();

        // TITLE (required)
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
            var assignee = Console.ReadLine() ?? "";
           assignee = assignee.Trim();
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

            DateTime date;
            if (DateTime.TryParse(dueStr, out date))
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
        Priority pr;
        if (!Enum.TryParse<Priority>(priority, true, out pr)) pr = Priority.Medium;
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
    }
}
