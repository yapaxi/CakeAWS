
Func<T> MODULE<T>(Func<T> t) => t; 

Func<T> METHOD<T>(Func<T> t) => t; 
Func<T1, T> METHOD<T1, T>(Func<T1, T> t) => t; 
Func<T1, T2, T> METHOD<T1, T2, T>(Func<T1, T2, T> t) => t; 
Func<T1, T2, T3, T> METHOD<T1, T2, T3, T>(Func<T1, T2, T3, T> t) => t; 
Func<T1, T2, T3, T4, T> METHOD<T1, T2, T3, T4, T>(Func<T1, T2, T3, T4, T> t) => t; 

const char __TASK_SYMBOL = '@';
CakeTaskBuilder<ActionTask> UniqueTask(string name) => Task(__ADD_GUID(name));
string __ADD_GUID(string name) => name?.Contains(__TASK_SYMBOL) == true
                         ? throw new InvalidOperationException($"{__TASK_SYMBOL} character is not allowed in a task name") 
                         : $"{name}{__TASK_SYMBOL}{Guid.NewGuid():N}";