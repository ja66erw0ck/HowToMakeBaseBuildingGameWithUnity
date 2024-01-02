using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using JobModel = Model.Job;

namespace Model.Manager
{
    public class Job : global::Model.Interface.IJsonSerializable
    {
        private Queue<JobModel> _jobs;
        
        public Job()
        {
            _jobs = new Queue<JobModel>();
        }

        public void Enqueue(JobModel job)
        {
            Debug.Log("Adding Job to Queue. Existing Queue Size: " + _jobs.Count);
            if (job.JobTime < 0) {
                // job has a negative job time, so it's not actually
                // supposed to be queued up.  just insta-complete it.
                job.DoWork(0);
                return;
            }    
            
            _jobs.Enqueue(job);

            CallbackJobCreated?.Invoke(job);
        }

        public JobModel Dequeue()
        {
            return _jobs.Count > 0 ? _jobs.Dequeue() : null;
        }
        
        public void Remove(JobModel job) {
            // TODO: Check docs to see if there's a less memory/swappy solution
            var jobs = new List<JobModel>(_jobs);
            
            if(jobs.Contains(job) == false) {
                //Debug.LogError("Trying to remove a job that doesn't exist on the queue.");
                // Most likely, this job wasn't on the queue because a character was working it
                return;
            }

            jobs.Remove(job);
            _jobs = new Queue<JobModel>(jobs);
        }
        
        // Callback 
        public Action<JobModel> CallbackJobCreated;
        
        public void RegisterJobCreated(Action<JobModel> callback)
        {
            CallbackJobCreated += callback;
        }

        public void UnregisterJobCreated(Action<JobModel> callback)
        {
            CallbackJobCreated -= callback;
        }
        
        /////////////////////////////////////////////////////////////////
        /// IJsonSerializable
        /////////////////////////////////////////////////////////////////

        public void FromJson(JToken token)
        {
            if (token == null) {
                return;
            } 
            
            foreach (var t in (JArray) token) {
                var x = (int) t["X"];
                var y = (int) t["Y"];
                var z = (int) t["Z"];
                var type = (string) t["Type"];
                var jobTimeRequired = (float) t["JobTimeRequired"];
                var isRepeat = (bool) t["IsRepeat"];

                /*
                Enqueue(new Job(
                    WorldModel.Current.GetTileModelAt(x, y, z),
                    type,
                    Action < JobModel > callbackJobCompleted,
                    jobTimeRequired,
                    , ItemModel[] itemRequirements,
                    isRepeat
                ));
                */
            }
        }

        public JToken ToJson()
        {
            var json = new JArray();
            foreach (var j in _jobs) {
                json.Add(j.ToJson());
            }
            return json;
        }
    }
}
