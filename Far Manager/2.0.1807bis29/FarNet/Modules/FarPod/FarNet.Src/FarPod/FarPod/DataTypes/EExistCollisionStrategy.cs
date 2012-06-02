namespace FarPod.DataTypes
{
    /// <summary>
    /// Strategy to resolve file/track exist collision
    /// </summary>
    enum EExistCollisionStrategy
    {
        None,
        Skip,
        Replace,
        Rename, // only for copy to PC        
        AddExistToPlayList, // only for copy to device
        Break,  
    }
}
