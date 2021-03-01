# Dining Philosophers
This is a university exercise.

## Usage
### Start
***dotnet run -p [numberOfPhilosophers] -t [maxThinkingTime] -e [maxEatingTime]***  
both *t* and *e* are in milliseconds.
### Stopping
Just use ***Ctrl+c*** after starting the program.
### Time Measurements
Displayed after Stopping the Program.

## Subtask 1: A naive implementation:
* Implemented in branch subtask1/naive-implementation

## Subtask 2: Deadlock prevention
* Implemented in main

Why does the deadlock occur? Answer the following questions:

1. What are the necessary conditions for deadlocks?
    * **Mutual exclusion**: limited number of threads may utilize a resource concurrently
    * **Hold and wait**: A thread holding a resource may request access to other resources and wait until it gets them
    * **No preemption**: Resources are released only voluntarily by the thread holding the resource (no stealing involved)
    * **Circular wait**: there is a set of { T1, .. Tn } threads, where T1 is waiting for a resource held by T2, T2 is waiting for a resource held by T3, and so forth,
      up to Tn waiting for a resource held by T1.  
    

2. Why does the initial solution lead to a deadlock (by looking at the deadlock conditions)?
    * **Mutual exclusion**: Every fork can be used by one philosopher (thread) only
    * **Hold and wait**: After taking (holding) the first fork (resource) a philosopher (thread) requests access to a second fork (resource) and waits until he/she gets it
    * **No preemption**: A philosopher (thread) may not steal the fork (resource) from another philosopher (thread)
    * **Circular wait**: The deadlock occurs exactly when philosopher p1 takes his first fork and requests his second fork which was already picked up by philosopher p2 which sets p1 into waiting.
      p2 in turn can not pick up his second fork because p3 already picked it up. p2 is in waiting state, and so forth.
      Up to pn waiting for his second fork which is already held by p1 (p1`s first fork).
      
Hint: use the following program arguments to produce a deadlock fast ***-p 2 -t 2 -e 2*** (having only 2 philosophers improves the probability of circular wait, while small t and e just execute faster)

Prevent the deadlock by removing Circular Wait condition:  
• Switch the order in which philosophers take the fork by using the following scheme: Odd philosophers start with the left fork, while even philosophers start with the right hand [6 points]. Make
sure to use concurrency primitives correctly!

• Does this strategy resolve the deadlock and why?
   * Yes it resolves the deadlock as not all deadlock conditions are satisfied anymore (no circular wait is possible anymore).  

• Measure the time spent in waiting for fork and compare it to the total runtime.
   * See [Stopping](#stopping)

• Can you think of other techniques for deadlock prevention?
   * One could treat both forks as one resource and only hold them together as one resource as well which would eliminate **Hold and wait**
   