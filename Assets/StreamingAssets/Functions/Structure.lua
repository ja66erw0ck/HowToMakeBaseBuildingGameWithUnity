-- Fireplace
function OnUpdateHit(structure, deltaTime)
    if (structure.Tile.Room == nil ) then
        return "structure's room was nil."
    end

    if (structure.Tile.Room.GetGasAmount("Temperature") < 30) then
        structure.Tile.Room.ChangeGas("Temperature", 0.01 * deltaTime)
    else
        -- Do we go into a standy mode to save power?
    end

    -- change animation
    if (structure.GetParameter("AnimationSpeed") > 0) then
        structure.ChangeParameter("AnimationSpeed", -deltaTime)
    else
        structure.SetParameter("AnimationSpeed", 1)
    end

    if (structure.CallbackOnChanged ~= nil) then
        structure.CallbackOnChanged(structure)
    end

    return
end

-- Door
function Clamp01(value)
    if (value > 1) then
        return 1
    elseif (value < 0) then
        return 0
    end
    return value
end

function OnUpdateDoor(structure, deltaTime)
    if (structure.GetParameter("IsOpening") >= 1) then
        structure.ChangeParameter("Openness", deltaTime)
        if (structure.GetParameter("Openness") >= 1) then
            structure.SetParameter("IsOpening", 0)
        end
    else
        structure.ChangeParameter("Openness", -deltaTime)
    end

    structure.SetParameter("Openness", Clamp01(structure.GetParameter("Openness")))

    if (structure.CallbackOnChanged ~= nil) then
        structure.CallbackOnChanged(structure)
    end
end

ENTERABILITY_YES    = 0
ENTERABILITY_NO     = 1
ENTERABILITY_SOON   = 2

function IsEnterableDoor(structure)
    structure.SetParameter("IsOpening", 1)

    if (structure.GetParameter("Openness") >= 1) then
        return ENTERABILITY_YES
    end

    return ENTERABILITY_SOON
end

-- Stockpile
function GetItemsFromStockpileFilter()
    return { Item.__new("Steel Plate", 50, 0) }
end

function OnUpdateStockpile(structure, deltaTime)
    if (structure.Tile.Item ~= nil and structure.Tile.Item.StackSize >= structure.Tile.Item.MaxStackSize) then
        structure.CancelJobs()
        return -- "We are full!"
    end

    if (structure.JobCount() > 0) then
        return
    end

    if (structure.Tile.Item ~= nil and structure.Tile.Item.StackSize == 0) then
        structure.CancelJobs()
        return "Stockpile has a zero-size stack. this is clearly wrong!"
    end

    itemsDesired = {}

    if (structure.Tile.Item == nil) then
        itemsDesired = GetItemsFromStockpileFilter()
    else
        desireItem = structure.Tile.Item.Clone()
        desireItem.MaxStackSize = desireItem.MaxStackSize - desireItem.StackSize
        desireItem.StackSize = 0

        itemsDesired = { desireItem }
    end

    job = Job.__new(structure.Tile, nil, nil, 0, itemsDesired, false)
    job.CanTakeFromStockpile = false

    job.RegisterJobWorked("JobWorkedStockpile")
    structure.AddJob(job)
end

function JobWorkedStockpile(job)
    job.CancelJob()

    for k, item in pairs(job.ItemRequirements) do
        if (item.StackSize > 0) then
            World.Current.ItemManager.Place(job.Tile, item)
            return
        end
    end
end

-- BrickMaker
function OnUpdateBrickMaker(structure, deltaTime)

    spawnSpotTile = structure.GetJobSpawnSpotTile()

    if (structure.JobCount() > 0) then
        if (spawnSpotTile.Item ~= nil and spawnSpotTile.Item.StackSize >= spawnSpotTile.Item.MaxStackSize) then
            structure.CancelJobs()
        end
        return
    end

    if (spawnSpotTile.Item ~= nil and spawnSpotTile.Item.StackSize >= spawnSpotTile.Item.MaxStackSize) then
        return
    end

    jobSpotTile = structure.GetJobSpotTile()

    job = Job.__new(jobSpotTile, nil, nil, 1, nil, true)
    job.RegisterJobCompleted("JobCompletedBrickMaker")
    structure.AddJob(job)
end

function JobCompletedBrickMaker(job)
    World.Current.ItemManager.Place(job.Structure.GetJobSpawnSpotTile(), Item.__new("Brick", 50, 10))
end


return "Lua Script Parsed"
