using System.Collections.Generic;
using UnityEngine;
using WorldModel = Model.World;
using JobModel = Model.Job;
using LayerType = Type.Layer;
using StructureSpriteController = Controller.Sprite.Structure;

namespace Controller.Sprite
{
    public class Job : MonoBehaviour
    {
        [SerializeField] private Transform jobTransform;
        
        // This bare-bones controller is mostly jout going to piggyback
        // on StructureSpriteController beacause we don't yet fully know
        // what our job system is going to look like in the end.
        
        public Dictionary<JobModel, GameObject> JobObjects;
        
        private StructureSpriteController _structureSpriteController;
        
        private void Start()
        {
            JobObjects = new Dictionary<JobModel, GameObject>();
            _structureSpriteController = FindObjectOfType<StructureSpriteController>();
            WorldModel.Current.JobManager.RegisterJobCreated(OnJobCreated);
        }

        private void OnJobCreated(JobModel job)
        {
            if (job.Type == null) {
                // this job doesn't really have an assoicated sprite with it, so no need to render
                return;
            }
            
            // FIXME: we can only do structure-building jobs
            
            // todo: sprite
            if (JobObjects.ContainsKey(job)) {
                //Debug.LogError("! job " + job.Type + " already queued!");
                return;
            }

            var jobObject = new GameObject {
                name = job.Type + "_" + job.Tile.X + "_" + job.Tile.Y + "_" + job.Tile.Z
            };
            
            var spriteRenderer = jobObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = _structureSpriteController.GetSpriteForStructure(job.Tile, job.Type, job.StructurePrototype);
            spriteRenderer.color = new Color(0.5f, 1f, 0.5f, 0.25f);
            
            // FIXME : set up so fast?
            jobObject.transform.position = new Vector3(
                job.Tile.X + (job.StructurePrototype.Width - 1 ) / 2f ,
                job.Tile.Y + (job.StructurePrototype.Height - 1 ) / 2f,
                job.Tile.Z - LayerType.Structure
            );
            jobObject.transform.SetParent(jobTransform, true);
          
            JobObjects.Add(job, jobObject);

            job.RegisterJobCompleted(OnJobEnded);
            job.RegisterJobStopped(OnJobEnded);
        }

        private void OnJobEnded(JobModel job)
        {
            // TODO: Delete sprite
            job.UnregisterJobCompleted(OnJobEnded);
            job.UnregisterJobStopped(OnJobEnded);
            
            var jobObject = JobObjects[job];
            Destroy(jobObject); 
        }
    }
}