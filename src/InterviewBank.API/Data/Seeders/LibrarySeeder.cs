using InterviewBank.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace InterviewBank.API.Data.Seeders;

public static class LibrarySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<AppDbContext>();

        if (await db.LibraryQuestions.AnyAsync()) return;

        var questions = new List<LibraryQuestion>
        {
            // ── JavaScript ────────────────────────────────────────────────────────
            new() { Id = Guid.NewGuid(), TopicName = "JavaScript", Difficulty = Difficulty.Easy,
                Text = "What is the difference between var, let, and const in JavaScript?",
                ExpectedAnswer = "`var` is function-scoped and hoisted (initialized to undefined). `let` is block-scoped and hoisted but not initialized (temporal dead zone). `const` is block-scoped, not re-assignable after declaration, but the value it points to can be mutated (e.g. object properties can change)." },

            new() { Id = Guid.NewGuid(), TopicName = "JavaScript", Difficulty = Difficulty.Easy,
                Text = "What is the difference between == and === in JavaScript?",
                ExpectedAnswer = "`==` performs type coercion before comparing — e.g. `'5' == 5` is true. `===` (strict equality) compares both value and type with no coercion — `'5' === 5` is false. Always prefer `===` to avoid unexpected coercion bugs." },

            new() { Id = Guid.NewGuid(), TopicName = "JavaScript", Difficulty = Difficulty.Easy,
                Text = "What is a closure in JavaScript?",
                ExpectedAnswer = "A closure is a function that retains access to its outer (enclosing) scope even after the outer function has returned. The inner function 'closes over' the variables of the outer function. Common use cases: data encapsulation, factory functions, and callbacks that need to remember state." },

            new() { Id = Guid.NewGuid(), TopicName = "JavaScript", Difficulty = Difficulty.Easy,
                Text = "What is the difference between null and undefined in JavaScript?",
                ExpectedAnswer = "`undefined` means a variable has been declared but not assigned a value, or a function parameter was not provided, or a missing object property. `null` is an intentional absence of value, set explicitly by the programmer. `typeof undefined === 'undefined'`, `typeof null === 'object'` (a historical JS bug)." },

            new() { Id = Guid.NewGuid(), TopicName = "JavaScript", Difficulty = Difficulty.Easy,
                Text = "What does the typeof operator return and for what types?",
                ExpectedAnswer = "`typeof` returns a string: 'undefined', 'boolean', 'number', 'bigint', 'string', 'symbol', 'function', or 'object'. Note that `typeof null` returns 'object' (a known bug), and `typeof []` also returns 'object'. Use `Array.isArray()` to detect arrays." },

            new() { Id = Guid.NewGuid(), TopicName = "JavaScript", Difficulty = Difficulty.Medium,
                Text = "Explain the JavaScript event loop and how asynchronous operations are handled.",
                ExpectedAnswer = "JavaScript is single-threaded. The event loop continuously checks the call stack and the task queue. Synchronous code runs on the call stack. When async operations (setTimeout, fetch, etc.) complete, their callbacks are pushed to the macrotask queue. Microtasks (Promise callbacks, queueMicrotask) have a separate, higher-priority queue that is drained completely after each task before the next macrotask runs." },

            new() { Id = Guid.NewGuid(), TopicName = "JavaScript", Difficulty = Difficulty.Medium,
                Text = "What is the difference between Promise.all(), Promise.allSettled(), Promise.race(), and Promise.any()?",
                ExpectedAnswer = "`Promise.all()` resolves when all promises resolve, rejects on the first rejection. `Promise.allSettled()` always resolves with an array of outcomes (fulfilled/rejected) for all promises. `Promise.race()` settles (resolve or reject) as soon as the first promise settles. `Promise.any()` resolves on the first fulfilled promise, rejects only if all reject (AggregateError)." },

            new() { Id = Guid.NewGuid(), TopicName = "JavaScript", Difficulty = Difficulty.Medium,
                Text = "Explain how 'this' works in JavaScript and how it changes in different contexts.",
                ExpectedAnswer = "'this' is determined at call time. In a method call `obj.method()`, this === obj. In a regular function (non-strict), this is the global object. In strict mode, this is undefined. Arrow functions do not have their own this — they inherit it from the enclosing lexical scope. `call()`, `apply()`, and `bind()` can explicitly set this." },

            new() { Id = Guid.NewGuid(), TopicName = "JavaScript", Difficulty = Difficulty.Medium,
                Text = "What is prototypal inheritance in JavaScript?",
                ExpectedAnswer = "Every JavaScript object has an internal [[Prototype]] link to another object (or null). When you access a property, JS first looks on the object itself, then walks up the prototype chain. `Object.create(proto)` creates an object with proto as its prototype. ES6 classes are syntactic sugar over this prototype mechanism — `class B extends A` sets up the prototype chain." },

            new() { Id = Guid.NewGuid(), TopicName = "JavaScript", Difficulty = Difficulty.Hard,
                Text = "Explain how JavaScript's garbage collection works and what causes memory leaks.",
                ExpectedAnswer = "V8 uses a generational garbage collector. Young objects live in 'new space' (semi-space copying collector). Objects that survive multiple GC cycles are promoted to 'old space' (mark-sweep/mark-compact). Memory leaks occur when references are retained unintentionally: global variables, detached DOM nodes held in JS, closures that capture large objects, accumulating event listeners, and timers/intervals that are never cleared." },

            new() { Id = Guid.NewGuid(), TopicName = "JavaScript", Difficulty = Difficulty.Hard,
                Text = "What are JavaScript Proxy and Reflect and what are their use cases?",
                ExpectedAnswer = "`Proxy` wraps an object and intercepts fundamental operations (get, set, has, deleteProperty, apply, construct, etc.) via 'traps'. `Reflect` provides the same operations as methods, making it easy to forward the default behaviour inside traps. Use cases: validation, reactive data (Vue 3 uses this), logging, access control, lazy loading, and memoization." },

            new() { Id = Guid.NewGuid(), TopicName = "JavaScript", Difficulty = Difficulty.Hard,
                Text = "Explain generators and how they differ from async/await.",
                ExpectedAnswer = "Generators are functions that can pause execution and yield multiple values one at a time using `function*` and `yield`. The caller controls resumption via `.next()`. `async/await` is built on top of generators and Promises — `await` is essentially yielding a Promise and resuming when it resolves. Generators are more general (can be used for custom iterables, infinite sequences, cooperative multitasking) while async/await is specialized for promise-based async control flow." },

            // ── C# ────────────────────────────────────────────────────────────────
            new() { Id = Guid.NewGuid(), TopicName = "C#", Difficulty = Difficulty.Easy,
                Text = "What is the difference between a class and a struct in C#?",
                ExpectedAnswer = "Classes are reference types: instances live on the heap and variables hold a reference. Structs are value types: instances are typically stored inline (stack or inside another object) and are copied on assignment. Classes support inheritance; structs do not (they can implement interfaces). Use structs for small, immutable, short-lived data (e.g. Point, DateTime) to avoid heap allocation pressure." },

            new() { Id = Guid.NewGuid(), TopicName = "C#", Difficulty = Difficulty.Easy,
                Text = "What is the difference between IEnumerable<T> and IQueryable<T>?",
                ExpectedAnswer = "`IEnumerable<T>` pulls data into memory and applies LINQ operators in-process (LINQ to Objects). `IQueryable<T>` represents a query that is translated to the data source's query language (e.g. SQL via EF Core) — filters, projections, and ordering happen on the server. Materialise `IQueryable<T>` only when needed to avoid loading the entire table into memory." },

            new() { Id = Guid.NewGuid(), TopicName = "C#", Difficulty = Difficulty.Easy,
                Text = "What is the difference between abstract and virtual methods in C#?",
                ExpectedAnswer = "A `virtual` method has a default implementation in the base class; derived classes may override it with `override`. An `abstract` method has no implementation in the base class; derived classes must override it. Abstract methods can only appear in abstract classes. Abstract classes cannot be instantiated directly." },

            new() { Id = Guid.NewGuid(), TopicName = "C#", Difficulty = Difficulty.Easy,
                Text = "What does the using statement do in C#?",
                ExpectedAnswer = "The `using` statement ensures that `Dispose()` is called on an `IDisposable` object when the block exits, even if an exception is thrown. The C# 8 `using` declaration (`using var x = ...;`) disposes at the end of the enclosing scope. It is equivalent to a try/finally that calls `Dispose()`." },

            new() { Id = Guid.NewGuid(), TopicName = "C#", Difficulty = Difficulty.Easy,
                Text = "What are nullable reference types in C# and why were they introduced?",
                ExpectedAnswer = "Enabled with `<Nullable>enable</Nullable>`, they make the compiler treat reference types as non-nullable by default, requiring explicit `?` to allow null (e.g. `string? name`). The compiler emits warnings when you dereference a potentially null value. They were introduced to eliminate NullReferenceException bugs at compile time rather than runtime." },

            new() { Id = Guid.NewGuid(), TopicName = "C#", Difficulty = Difficulty.Medium,
                Text = "Explain async/await in C# and how it differs from creating a new Thread.",
                ExpectedAnswer = "`async/await` is built on top of the Task Parallel Library and uses a state machine generated by the compiler. When you `await` an incomplete Task, the current thread is released back to the pool — no thread is blocked. Threads are OS resources and are expensive. `async/await` is ideal for I/O-bound work (network, disk) where you'd otherwise block a thread while waiting. `new Thread()` creates an OS thread and is better suited for CPU-bound work that must run in parallel." },

            new() { Id = Guid.NewGuid(), TopicName = "C#", Difficulty = Difficulty.Medium,
                Text = "What is LINQ deferred execution and when does execution actually occur?",
                ExpectedAnswer = "Most LINQ operators (Where, Select, OrderBy, etc.) return an `IEnumerable<T>` or `IQueryable<T>` that represents a query definition — no data is read yet. Execution happens when you enumerate the result: a foreach loop, `.ToList()`, `.ToArray()`, `.First()`, `.Count()`, etc. This allows query composition without multiple database round-trips, but means the query runs again each time you enumerate unless you materialize it." },

            new() { Id = Guid.NewGuid(), TopicName = "C#", Difficulty = Difficulty.Medium,
                Text = "Explain dependency injection in ASP.NET Core and the three service lifetimes.",
                ExpectedAnswer = "ASP.NET Core has a built-in IoC container. Services are registered in `Program.cs` with three lifetimes: **Singleton** — one instance for the whole app; **Scoped** — one instance per HTTP request; **Transient** — a new instance every time it is resolved. Constructor injection is the primary pattern. Injecting a Scoped or Transient service into a Singleton (captive dependency) is a common pitfall." },

            new() { Id = Guid.NewGuid(), TopicName = "C#", Difficulty = Difficulty.Medium,
                Text = "What is the difference between Task and ValueTask in C#?",
                ExpectedAnswer = "`Task` is a heap-allocated reference type. `ValueTask` is a struct that avoids allocation when the result is already available synchronously (common for cache hits, hot paths). `ValueTask` should not be awaited more than once. Use `ValueTask` in high-throughput library code where allocation pressure matters; prefer `Task` elsewhere for simplicity." },

            new() { Id = Guid.NewGuid(), TopicName = "C#", Difficulty = Difficulty.Hard,
                Text = "Explain how the .NET garbage collector works, including the concept of generations.",
                ExpectedAnswer = ".NET's GC is a generational, compacting collector. Gen 0 is collected most frequently (short-lived objects). Objects that survive a Gen 0 collection are promoted to Gen 1; survivors of Gen 1 go to Gen 2 (long-lived). Large objects (≥85 KB) go directly to the Large Object Heap (LOH), which is not compacted by default. The GC uses write barriers to track inter-generational references. GC may pause all threads ('stop the world') but workstation/server and background GC modes reduce pauses." },

            new() { Id = Guid.NewGuid(), TopicName = "C#", Difficulty = Difficulty.Hard,
                Text = "What are expression trees in C# and how does Entity Framework Core use them?",
                ExpectedAnswer = "Expression trees (System.Linq.Expressions) represent code as data — an in-memory tree of nodes (MethodCallExpression, BinaryExpression, etc.) that can be inspected and translated at runtime. When you write `IQueryable<T>` LINQ queries, the lambda predicates are compiled to expression trees rather than delegates. EF Core's query translator walks these trees to produce SQL. You can also build and compile expression trees dynamically for runtime code generation." },

            new() { Id = Guid.NewGuid(), TopicName = "C#", Difficulty = Difficulty.Hard,
                Text = "Explain Span<T> and Memory<T> and when to use each.",
                ExpectedAnswer = "`Span<T>` is a ref struct that represents a contiguous region of memory (array slice, stack memory, native memory) without copying. It is stack-only — cannot be boxed, stored in fields, or used across await points. `Memory<T>` is a struct that can be stored on the heap; it can be used across async boundaries. Both enable zero-copy slicing and are key to high-performance code that avoids allocations (e.g. parsing, serialization)." },

            new() { Id = Guid.NewGuid(), TopicName = "C#", Difficulty = Difficulty.Hard,
                Text = "What is the difference between covariance and contravariance in C# generics?",
                ExpectedAnswer = "Covariance (`out T`) allows using a more derived type than specified — e.g. `IEnumerable<Cat>` can be assigned to `IEnumerable<Animal>`. Only safe on output positions (return types). Contravariance (`in T`) allows a less derived type — e.g. `Action<Animal>` can be assigned to `Action<Cat>`. Only safe on input positions (parameters). Invariance (neither) means the generic type must match exactly. Arrays in C# are covariant but unsafely so (ArrayTypeMismatchException at runtime)." },

            // ── SQL ───────────────────────────────────────────────────────────────
            new() { Id = Guid.NewGuid(), TopicName = "SQL", Difficulty = Difficulty.Easy,
                Text = "What is the difference between INNER JOIN, LEFT JOIN, and RIGHT JOIN?",
                ExpectedAnswer = "`INNER JOIN` returns only rows where there is a match in both tables. `LEFT JOIN` returns all rows from the left table plus matching rows from the right; non-matching right columns are NULL. `RIGHT JOIN` is the mirror: all rows from the right table, NULLs for non-matching left columns. `FULL OUTER JOIN` returns all rows from both, NULLs where there is no match on either side." },

            new() { Id = Guid.NewGuid(), TopicName = "SQL", Difficulty = Difficulty.Easy,
                Text = "What is the difference between WHERE and HAVING in SQL?",
                ExpectedAnswer = "`WHERE` filters rows before aggregation — it cannot reference aggregate functions. `HAVING` filters groups after `GROUP BY` aggregation — it can reference aggregate functions like `COUNT()`, `SUM()`, `AVG()`. You can use both in the same query: WHERE reduces rows first, then GROUP BY aggregates, then HAVING filters the groups." },

            new() { Id = Guid.NewGuid(), TopicName = "SQL", Difficulty = Difficulty.Easy,
                Text = "What is the difference between DELETE, TRUNCATE, and DROP?",
                ExpectedAnswer = "`DELETE` removes rows matching a WHERE clause (or all rows if omitted), is logged per-row, fires triggers, and can be rolled back. `TRUNCATE` removes all rows without logging per-row — much faster, resets identity counters, cannot be filtered, may not fire triggers. `DROP` removes the entire table structure and its data permanently." },

            new() { Id = Guid.NewGuid(), TopicName = "SQL", Difficulty = Difficulty.Easy,
                Text = "What is a primary key and how does it differ from a unique key?",
                ExpectedAnswer = "A primary key uniquely identifies each row in a table. It implicitly creates a unique index and does NOT allow NULL. A table can have only one primary key (which may be composite). A unique key also enforces uniqueness and creates a unique index, but allows one NULL per column (behaviour varies by DBMS). A table can have multiple unique constraints." },

            new() { Id = Guid.NewGuid(), TopicName = "SQL", Difficulty = Difficulty.Easy,
                Text = "What are database indexes and how do B-tree indexes work?",
                ExpectedAnswer = "An index is a separate data structure that speeds up lookups at the cost of additional storage and slower writes. B-tree (balanced tree) indexes maintain sorted key values with pointers to rows, supporting equality and range queries in O(log n). A clustered index determines the physical row order (only one per table). Non-clustered indexes store key + row pointer separately." },

            new() { Id = Guid.NewGuid(), TopicName = "SQL", Difficulty = Difficulty.Medium,
                Text = "What is the N+1 query problem and how do you solve it?",
                ExpectedAnswer = "N+1 occurs when you load N parent records and then execute one additional query per record to load related data — e.g. loading 100 orders then 100 queries for each order's items = 101 queries. Solutions: use a JOIN to load both in one query; use eager loading (`Include()` in EF Core); use batch loading (IN clause); or use a dedicated ORM feature like DataLoader (GraphQL). The problem is common with ORMs in lazy-loading mode." },

            new() { Id = Guid.NewGuid(), TopicName = "SQL", Difficulty = Difficulty.Medium,
                Text = "What are the ACID properties of database transactions?",
                ExpectedAnswer = "**Atomicity**: the transaction either completes fully or not at all — no partial updates. **Consistency**: the database moves from one valid state to another, respecting all constraints. **Isolation**: concurrent transactions behave as if they ran serially — intermediate state is not visible to others. **Durability**: once committed, data survives crashes (written to durable storage). These properties are enforced through logging (WAL), locks, and MVCC." },

            new() { Id = Guid.NewGuid(), TopicName = "SQL", Difficulty = Difficulty.Medium,
                Text = "What is a CTE (Common Table Expression) and when is it useful?",
                ExpectedAnswer = "A CTE is a named temporary result set defined with `WITH name AS (SELECT ...)` at the top of a query. It improves readability over subqueries, can be referenced multiple times in the same query, and supports recursion (`WITH RECURSIVE`) for hierarchical data (trees, graphs). CTEs are not materialized by default in most databases — the optimizer may inline them." },

            new() { Id = Guid.NewGuid(), TopicName = "SQL", Difficulty = Difficulty.Medium,
                Text = "Explain the first three normal forms of database normalization.",
                ExpectedAnswer = "**1NF**: each column holds atomic (indivisible) values; no repeating groups; rows are uniquely identifiable. **2NF**: must be in 1NF, and every non-key attribute must depend on the entire primary key (no partial dependency on a composite key). **3NF**: must be in 2NF, and every non-key attribute must depend directly on the primary key, not on another non-key attribute (no transitive dependency)." },

            new() { Id = Guid.NewGuid(), TopicName = "SQL", Difficulty = Difficulty.Hard,
                Text = "Explain database isolation levels and the anomalies each prevents.",
                ExpectedAnswer = "**Read Uncommitted**: reads dirty (uncommitted) data. **Read Committed**: prevents dirty reads; allows non-repeatable reads and phantoms. **Repeatable Read**: prevents dirty reads and non-repeatable reads; phantoms may occur (MySQL InnoDB blocks them with gap locks). **Serializable**: full isolation — prevents all anomalies by making transactions appear serial. Higher isolation levels reduce concurrency and may increase lock contention. PostgreSQL uses MVCC (snapshot isolation) instead of traditional locking." },

            new() { Id = Guid.NewGuid(), TopicName = "SQL", Difficulty = Difficulty.Hard,
                Text = "What are SQL window functions and how do they differ from GROUP BY aggregates?",
                ExpectedAnswer = "Window functions compute a value across a set of rows related to the current row without collapsing the result into fewer rows. Syntax: `FUNCTION() OVER (PARTITION BY ... ORDER BY ...)`. Examples: `ROW_NUMBER()`, `RANK()`, `LAG()`, `LEAD()`, `SUM() OVER (...)`. Unlike `GROUP BY`, all original rows are preserved and you can access both the aggregated value and the row's own columns in the same SELECT." },

            new() { Id = Guid.NewGuid(), TopicName = "SQL", Difficulty = Difficulty.Hard,
                Text = "What is the difference between optimistic and pessimistic locking?",
                ExpectedAnswer = "**Pessimistic locking** acquires a lock before reading data to prevent concurrent modification (`SELECT ... FOR UPDATE`). Safe but reduces concurrency; risk of deadlocks. **Optimistic locking** reads data without locking and checks for conflicts at write time — typically by comparing a version column or timestamp. If the version changed, the update fails and the operation must retry. Optimistic is better for low-contention scenarios; pessimistic is better when conflicts are frequent." },

            new() { Id = Guid.NewGuid(), TopicName = "SQL", Difficulty = Difficulty.Hard,
                Text = "What is a covering index and how does it improve query performance?",
                ExpectedAnswer = "A covering index includes all columns referenced by a query (in the index's key or INCLUDE columns), so the query engine can satisfy the query entirely from the index without touching the base table (avoids a 'key lookup' or 'bookmark lookup'). For example, an index on `(UserId, Difficulty) INCLUDE (Text, TopicId)` covers a query that filters on UserId/Difficulty and selects Text and TopicId. This eliminates a second I/O to the table's data pages." },

            // ── Algorithms ────────────────────────────────────────────────────────
            new() { Id = Guid.NewGuid(), TopicName = "Algorithms", Difficulty = Difficulty.Easy,
                Text = "What is Big O notation and what do O(1), O(log n), O(n), and O(n log n) mean?",
                ExpectedAnswer = "Big O describes the upper bound of an algorithm's growth rate relative to input size n, ignoring constants. O(1) — constant time regardless of input (array index lookup). O(log n) — halves the problem each step (binary search). O(n) — linear, visits each element once (linear scan). O(n log n) — typically divide-and-conquer sorting (merge sort, heap sort). O(n²) — nested loops over the input (bubble sort, brute-force pair search)." },

            new() { Id = Guid.NewGuid(), TopicName = "Algorithms", Difficulty = Difficulty.Easy,
                Text = "What is binary search and what precondition does it require?",
                ExpectedAnswer = "Binary search finds a target in a sorted array in O(log n) by repeatedly halving the search space. It compares the target to the middle element: if equal, done; if less, search the left half; if greater, search the right half. Precondition: the array must be sorted. For an unsorted array you must sort first (O(n log n)) or use a linear scan." },

            new() { Id = Guid.NewGuid(), TopicName = "Algorithms", Difficulty = Difficulty.Easy,
                Text = "What is the difference between a stack and a queue?",
                ExpectedAnswer = "A **stack** is LIFO (Last In First Out) — push adds to the top, pop removes from the top. Use cases: recursion call stack, undo/redo, expression parsing. A **queue** is FIFO (First In First Out) — enqueue adds to the back, dequeue removes from the front. Use cases: BFS traversal, task scheduling, message queues. Both support O(1) push/enqueue and pop/dequeue when implemented with a linked list or a circular array." },

            new() { Id = Guid.NewGuid(), TopicName = "Algorithms", Difficulty = Difficulty.Easy,
                Text = "What is a hash table and how does it handle collisions?",
                ExpectedAnswer = "A hash table maps keys to values using a hash function that computes an array index. Average O(1) for get/put/delete. Collisions (two keys mapping to the same index) are handled by: **chaining** — each bucket holds a linked list of entries; **open addressing** — probe for the next empty slot (linear probing, quadratic probing, double hashing). Performance degrades as the load factor (entries/capacity) increases; resizing (rehashing) keeps it low." },

            new() { Id = Guid.NewGuid(), TopicName = "Algorithms", Difficulty = Difficulty.Easy,
                Text = "What is the difference between an array and a linked list?",
                ExpectedAnswer = "**Array**: contiguous memory, O(1) index access, O(n) insertion/deletion in the middle (shifting), fixed size (or amortized O(1) for dynamic arrays). **Linked list**: nodes with pointers, O(n) index access (must traverse), O(1) insertion/deletion once you have the node, no contiguous memory needed. Arrays have better cache locality; linked lists have better write performance for frequent insertions/deletions." },

            new() { Id = Guid.NewGuid(), TopicName = "Algorithms", Difficulty = Difficulty.Medium,
                Text = "Explain how quicksort works and its average vs worst-case time complexity.",
                ExpectedAnswer = "Quicksort picks a pivot, partitions the array so elements less than the pivot are left and greater are right, then recursively sorts the two halves. Average case: O(n log n) — balanced partitions. Worst case: O(n²) — consistently bad pivots (already sorted array with first/last element pivot). Mitigated with random pivot selection or median-of-three. In-place (O(log n) stack), not stable. Typically faster than merge sort in practice due to cache locality." },

            new() { Id = Guid.NewGuid(), TopicName = "Algorithms", Difficulty = Difficulty.Medium,
                Text = "What is dynamic programming and how does memoization differ from tabulation?",
                ExpectedAnswer = "Dynamic programming solves problems by breaking them into overlapping subproblems and storing results to avoid recomputation. **Memoization** (top-down): recursively solve subproblems, cache results in a map — only computes subproblems that are actually needed. **Tabulation** (bottom-up): fill a table iteratively in a defined order, computing all subproblems. Tabulation avoids recursion overhead; memoization is often more natural to write. Both achieve O(subproblems × cost per subproblem)." },

            new() { Id = Guid.NewGuid(), TopicName = "Algorithms", Difficulty = Difficulty.Medium,
                Text = "What is the difference between BFS and DFS, and when would you use each?",
                ExpectedAnswer = "**BFS** (Breadth-First Search) explores neighbours level by level using a queue. Finds the shortest path in unweighted graphs. Memory: O(w) where w is the maximum width. **DFS** (Depth-First Search) explores as far as possible before backtracking, using a stack (or recursion). Better for detecting cycles, topological sort, connected components, exploring all paths. Memory: O(h) where h is depth. BFS guarantees shortest path; DFS does not." },

            new() { Id = Guid.NewGuid(), TopicName = "Algorithms", Difficulty = Difficulty.Medium,
                Text = "How does a binary heap work and how is it used to implement a priority queue?",
                ExpectedAnswer = "A binary heap is a complete binary tree stored in an array where each node satisfies the heap property (max-heap: parent ≥ children). Insert: add at the end, bubble up (O(log n)). Extract-max: swap root with last element, remove last, sift down (O(log n)). Peek: O(1). Heap sort uses this structure in-place. A priority queue wraps a heap to always dequeue the highest-priority element. Build heap from unsorted array is O(n)." },

            new() { Id = Guid.NewGuid(), TopicName = "Algorithms", Difficulty = Difficulty.Hard,
                Text = "Explain Dijkstra's algorithm and its time complexity with different data structures.",
                ExpectedAnswer = "Dijkstra's finds the shortest path from a source to all vertices in a weighted graph with non-negative edges. It greedily picks the unvisited vertex with the smallest tentative distance, then relaxes its neighbours. With an adjacency list and binary min-heap: O((V + E) log V). With a Fibonacci heap: O(E + V log V). With an adjacency matrix and linear scan for minimum: O(V²). Does not work with negative edge weights (use Bellman-Ford instead)." },

            new() { Id = Guid.NewGuid(), TopicName = "Algorithms", Difficulty = Difficulty.Hard,
                Text = "Explain amortized time complexity using dynamic array resizing as an example.",
                ExpectedAnswer = "Amortized analysis averages the cost over a sequence of operations rather than looking at the worst case of a single operation. When a dynamic array is full, it doubles its capacity and copies all elements — O(n) for that one push. But this expensive operation is rare: after doubling, the next n pushes are O(1). Total cost for n pushes = O(n) copies + O(n) pushes = O(n). Amortized cost per push = O(n)/n = O(1). Similar analysis applies to hash table resizing and stack operations on incrementally resizing buffers." },

            new() { Id = Guid.NewGuid(), TopicName = "Algorithms", Difficulty = Difficulty.Hard,
                Text = "What is a trie data structure and what are its advantages for string operations?",
                ExpectedAnswer = "A trie (prefix tree) is a tree where each node represents a character, and paths from the root to a leaf represent a word. Insert/search/delete all run in O(L) where L is the string length — independent of the number of strings stored. Advantages over a hash set for strings: O(L) lookup, prefix search/autocomplete in O(P + results), lexicographic ordering naturally, no hash collision risk. Disadvantage: higher memory usage than a hash set for sparse key sets (mitigated with compressed tries / radix trees)." },

            new() { Id = Guid.NewGuid(), TopicName = "Algorithms", Difficulty = Difficulty.Hard,
                Text = "Explain how consistent hashing works and why it is used in distributed systems.",
                ExpectedAnswer = "Consistent hashing maps both nodes and keys onto a circular hash ring (0 to 2^32). Each key is stored on the first node clockwise from its hash position. Adding or removing a node only reassigns the keys on the arc between the removed/added node and its predecessor — O(K/N) keys remapped instead of O(K) with modular hashing. Virtual nodes (each physical node appears multiple times on the ring) improve load distribution. Used in distributed caches (Cassandra, DynamoDB), CDNs, and load balancers." },
        };

        db.LibraryQuestions.AddRange(questions);
        await db.SaveChangesAsync();
    }
}
