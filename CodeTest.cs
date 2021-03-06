﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BTCodeTest
{
    /// <summary>
    /// Class containing the main method
    /// </summary>
    public class CodeTest
    {
        public static void Main(string[] args)
        {
            // Read file
            // Close file
            // Process the input
            string fileName = string.Empty;

            if (args.Length != 0)
            {
                fileName = args[0];
            }
            
            // Validate User Input and file name
            while (string.IsNullOrEmpty(fileName.Trim()) || !File.Exists(fileName))
            {
                Console.WriteLine("Please specify a valid file name to read from.");
                fileName = Console.ReadLine();
            }

            // Read File
            string[] lines = File.ReadAllLines(fileName);

            // Get the valid session entry
            List<SessionDetail> sessionDetail = new List<SessionDetail>();
            List<SessionDetail> userSession = new List<SessionDetail>();
            SessionOutput result = new SessionOutput();

            foreach (var line in lines)
            {
                var lineData = line.Trim().Split(' ');

                // Check for valid records. Valid records contains Time UserName Start/End
                if (lineData.Length != 3)
                {
                    continue;
                }

                // Get the seeion date time stamp
                DateTime sessionTime;
                if(!DateTime.TryParse(lineData[0], out sessionTime))
                {
                    continue;
                }

                // Check if start and end is mentioned
                if (!lineData[2].ToLower().Equals("start") && !lineData[2].ToLower().Equals("end"))
                {
                    continue;
                }

                if (string.IsNullOrEmpty(lineData[1]))
                {
                    continue;
                }
                
                // Add to the Session Details list
                sessionDetail.Add(new SessionDetail {
                    SessionTime = sessionTime,
                    UserName = lineData[1],
                    IsStart = lineData[2].ToLower().Equals("start")
                });
            }

            sessionDetail.Sort();
            var dayStartTime = sessionDetail[0].SessionTime;
            var dayEndTime = sessionDetail[sessionDetail.Count - 1].SessionTime;

            // Get the user list
            var userList = sessionDetail.Select(x => x.UserName).Distinct();
            foreach (var user in userList)
            {
                userSession = sessionDetail.Where(x => x.UserName.Equals(user)).ToList();
                result = GetNumberOfSessionsAndDuration(userSession, dayStartTime, dayEndTime);
                Console.WriteLine(user + " " + result.NumberOfSessions + " " + result.TotalDuration);
            }
            Console.ReadLine();
        }

        /// <summary>
        /// Calculates the number of session and total session duration
        /// </summary>
        /// <param name="userSession">user session list</param>
        /// <param name="dayStart">day start time</param>
        /// <param name="dayEnd">day end time</param>
        /// <returns>SessionOutput object</returns>
        public static SessionOutput GetNumberOfSessionsAndDuration(List<SessionDetail> userSession, DateTime dayStart, DateTime dayEnd)
        {
            var sessionOutput = new SessionOutput();
            List<SessionDetail> startLog, endLog, startLogCopy, endLogCopy;
            startLog = userSession.Where(x => x.IsStart).ToList();
            endLog = userSession.Where(x => !x.IsStart).ToList();
            startLogCopy = startLog.ToList();
            endLogCopy = endLog.ToList();
            var startCount = startLog.Count;
            var endCount = endLog.Count;
            
            // Calculate the session
            for (var i = 0; i < endLogCopy.Count; i++)
            {
                var logIndex = 0;
                for (var j = 0; j < startLogCopy.Count;)
                {
                    if (DateTime.Compare(endLogCopy[i].SessionTime, startLogCopy[j].SessionTime) > 0)
                    {
                        sessionOutput.TotalDuration = sessionOutput.TotalDuration + (endLogCopy[i].SessionTime - startLogCopy[j].SessionTime).TotalSeconds;
                        logIndex = j;
                        sessionOutput.NumberOfSessions = sessionOutput.NumberOfSessions + 1;
                        break;
                    }

                    sessionOutput.TotalDuration = sessionOutput.TotalDuration + (endLogCopy[i].SessionTime - dayStart).TotalSeconds;
                    sessionOutput.NumberOfSessions = sessionOutput.NumberOfSessions + 1;
                    logIndex = -1;
                    break;
                }

                if (logIndex >= 0 && startLogCopy.Count > logIndex)
                {
                    startLogCopy.RemoveAt(logIndex);
                    endLog.Remove(endLogCopy[i]);
                }

                if(logIndex == -1)
                {
                    endLog.Remove(endLogCopy[i]);
                }
            }

            foreach (var log in startLogCopy)
            {
                sessionOutput.TotalDuration = sessionOutput.TotalDuration + (dayEnd - log.SessionTime).TotalSeconds;
                sessionOutput.NumberOfSessions = sessionOutput.NumberOfSessions + 1;
            }

            foreach (var log in endLog)
            {
                sessionOutput.TotalDuration = sessionOutput.TotalDuration + (log.SessionTime - dayStart).TotalSeconds;
                sessionOutput.NumberOfSessions = sessionOutput.NumberOfSessions + 1;
            }

            return sessionOutput;
        }        
    }

    /// <summary>
    /// Class to hold the session details
    /// </summary>
    public class SessionDetail : IComparable
    {
        public DateTime SessionTime { get; set; }
        public string UserName { get; set; }
        public bool IsStart { get; set; }

        public int CompareTo(object obj)
        {
            if (obj is SessionDetail)
            {
                return this.SessionTime.CompareTo((obj as SessionDetail).SessionTime);
            }
            throw new ArgumentException("Object is not a SessionDetails");
        }
    }

    /// <summary>
    /// Class to hold user session result
    /// </summary>
    public class SessionOutput
    {
        public int NumberOfSessions { get; set; }
        public double TotalDuration { get; set; }
    }
}
