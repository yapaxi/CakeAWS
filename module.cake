
Action<T> METHOD<T1, T>(Action<T> t) => t; 
Func<T> MODULE<T>(Func<T> t) => t; 
Func<T> METHOD<T>(Func<T> t) => t; 
Func<T1, T> METHOD<T1, T>(Func<T1, T> t) => t; 
Func<T1, T2, T> METHOD<T1, T2, T>(Func<T1, T2, T> t) => t; 
Func<T1, T2, T3, T> METHOD<T1, T2, T3, T>(Func<T1, T2, T3, T> t) => t; 
Func<T1, T2, T3, T4, T> METHOD<T1, T2, T3, T4, T>(Func<T1, T2, T3, T4, T> t) => t; 