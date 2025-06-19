using ITR_TelementaryAPI.UDP;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;

// Demonstrate a Priority Queue implemented with a Binary Heap

namespace ITRTelemetry.Models
{
    public class PriorityQueuesProgram
    {
        // Main()
        public void TestPriorityQueues()
        {
            //System.Diagnostics.Debug.WriteLine("\nBegin Priority Queue demo");      
            //System.Diagnostics.Debug.WriteLine("\nCreating priority queue of Telemetry Stations\n");
            PriorityQueue<TelemetryStation> pq = new PriorityQueue<TelemetryStation>();
            MySqlConnection con = new MySqlConnection("server=localhost;userid=root;pwd=N33mu$123;port=3306;database=itr;sslmode=none");
            con.Open();
            MySqlCommand cmd = new MySqlCommand("SELECT TSID,Name from telemetrystationtable", con);

            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            int arrsize = dt.Rows.Count;
            var data = new TelemetryStation[arrsize];
            if (dt.Rows.Count > 0)
            {
                for (var i = 0; i < dt.Rows.Count; i++)
                {

                    int tsid = Convert.ToInt32(dt.Rows[i]["TSID"].ToString());
                    var name = dt.Rows[i]["Name"].ToString();
                    data[i] = new TelemetryStation
                    (
                       tsid,
                       name
                    );
                }

            }
            for (int i = 1; i < data.Length; i++)
            {
                // System.Diagnostics.Debug.WriteLine("Adding " + data[i].Name.ToString() + " to priority queue");
                pq.Enqueue(data[i]);
            }

            System.Diagnostics.Debug.WriteLine("\nPriory queue is: ");
            System.Diagnostics.Debug.WriteLine(pq.ToString());
            System.Diagnostics.Debug.WriteLine("\n");

            System.Diagnostics.Debug.WriteLine(pq.Count());
            TelemetryStation e = pq.Dequeue();
            UDPSocket s = new UDPSocket();
           // s.Server("192.168.1.19", 9000);

            UDPSocket c = new UDPSocket();
          //  c.Client("192.168.1.19", 63554);
            c.Send("TEST!");

            System.Diagnostics.Debug.WriteLine("Selected telemetry station from priority queue is " + e.ToString());
            System.Diagnostics.Debug.WriteLine("Processing telemetry station");
            System.Diagnostics.Debug.WriteLine("Removed telemetry station is " + e.ToString());
            System.Diagnostics.Debug.WriteLine("\nPriory queue is now: ");
            System.Diagnostics.Debug.WriteLine(pq.ToString());
            System.Diagnostics.Debug.WriteLine("\n");

            System.Diagnostics.Debug.WriteLine("Removing a telemetry station from queue");
            e = pq.Dequeue();
            System.Diagnostics.Debug.WriteLine("\nPriory queue is now: ");
            System.Diagnostics.Debug.WriteLine(pq.ToString());
            System.Diagnostics.Debug.WriteLine("\n");

            System.Diagnostics.Debug.WriteLine("Testing the priority queue");
            TestPriorityQueue(50000);


            System.Diagnostics.Debug.WriteLine("\nEnd Priority Queue demo");
            Console.ReadLine();

        }
        static void TestPriorityQueue(int numOperations)
        {
            Random rand = new Random(0);
            PriorityQueue<TelemetryStation> pq = new PriorityQueue<TelemetryStation>();
            for (int op = 0; op < numOperations; ++op)
            {
                int opType = rand.Next(0, 2);

                if (opType == 0) // enqueue
                {
                    string Name = op + "man";
                    int TSID = (100 - 1) * rand.Next() + 1;
                    pq.Enqueue(new TelemetryStation(TSID, Name));
                    if (pq.IsConsistent() == false)
                    {
                        System.Diagnostics.Debug.WriteLine("Test fails after enqueue operation # " + op);
                    }
                }
                else // dequeue
                {
                    if (pq.Count() > 0)
                    {
                        TelemetryStation e = pq.Dequeue();
                        if (pq.IsConsistent() == false)
                        {
                            System.Diagnostics.Debug.WriteLine("Test fails after dequeue operation # " + op);
                        }
                    }
                }
            } // for
            System.Diagnostics.Debug.WriteLine("\nAll tests passed");
        } // TestPriorityQueue

    } // class PriorityQueuesProgram

    // ===================================================================
    public class TelemetryStation : IComparable<TelemetryStation>
    {
        public int TSID { get; set; }
        public string Name { get; set; }
        public TelemetryStation(int TSID, string Name)
        {
            this.TSID = TSID;
            this.Name = Name;
        }

        public override string ToString()
        {
            return "(" + TSID + ", " + Name + ")";
        }
        public int CompareTo(TelemetryStation other)
        {
            if (this.TSID < other.TSID) return -1;
            else if (this.TSID > other.TSID) return 1;
            else return 0;
        }

    }
    //  public class Employee : IComparable<Employee>
    //{
    //  public string lastName;
    //  public double priority; // smaller values are higher priority


    //      public Employee(string lastName, double priority)
    //  {
    //    this.lastName = lastName;
    //    this.priority = priority;
    //  }

    //  public override string ToString()
    //  {
    //    return "(" + lastName + ", " + priority.ToString("F1") + ")";
    //  }

    //  public int CompareTo(Employee other)
    //  {
    //    if (this.priority < other.priority) return -1;
    //    else if (this.priority > other.priority) return 1;
    //    else return 0;
    //  }
    //} // Employee

    // ===================================================================

    public class PriorityQueue<T> where T : IComparable<T>
    {
        private List<T> data;

        public PriorityQueue()
        {
            this.data = new List<T>();
        }

        public void Enqueue(T item)
        {
            data.Add(item);
            int ci = data.Count - 1; // child index; start at end
            while (ci > 0)
            {
                int pi = (ci - 1) / 2; // parent index
                if (data[ci].CompareTo(data[pi]) >= 0) break; // child item is larger than (or equal) parent so we're done
                T tmp = data[ci]; data[ci] = data[pi]; data[pi] = tmp;
                ci = pi;
            }
        }

        public T Dequeue()
        {
            // assumes pq is not empty; up to calling code
            int li = data.Count - 1; // last index (before removal)
            T frontItem = data[0];   // fetch the front
            data[0] = data[li];
            data.RemoveAt(li);

            --li; // last index (after removal)
            int pi = 0; // parent index. start at front of pq
            while (true)
            {
                int ci = pi * 2 + 1; // left child index of parent
                if (ci > li) break;  // no children so done
                int rc = ci + 1;     // right child
                if (rc <= li && data[rc].CompareTo(data[ci]) < 0) // if there is a rc (ci + 1), and it is smaller than left child, use the rc instead
                    ci = rc;
                if (data[pi].CompareTo(data[ci]) <= 0) break; // parent is smaller than (or equal to) smallest child so done
                T tmp = data[pi]; data[pi] = data[ci]; data[ci] = tmp; // swap parent and child
                pi = ci;
            }
            return frontItem;
        }

        public T Peek()
        {
            T frontItem = data[0];
            return frontItem;
        }

        public int Count()
        {
            return data.Count;
        }

        public override string ToString()
        {
            string s = "";
            for (int i = 0; i < data.Count; ++i)
                s += data[i].ToString() + " ";
            s += "count = " + data.Count;
            return s;
        }

        public bool IsConsistent()
        {
            // is the heap property true for all data?
            if (data.Count == 0) return true;
            int li = data.Count - 1; // last index
            for (int pi = 0; pi < data.Count; ++pi) // each parent index
            {
                int lci = 2 * pi + 1; // left child index
                int rci = 2 * pi + 2; // right child index

                if (lci <= li && data[pi].CompareTo(data[lci]) > 0) return false; // if lc exists and it's greater than parent then bad.
                if (rci <= li && data[pi].CompareTo(data[rci]) > 0) return false; // check the right child too.
            }
            return true; // passed all checks
        } // IsConsistent
    } // PriorityQueue

} // ns
