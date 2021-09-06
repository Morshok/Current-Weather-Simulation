public class SphericalCoordinate
{
    private float Longitude { get; set; }
    private float Latitude { get; set; }

    public SphericalCoordinate(float Longitude, float Latitude)
    {
        this.Longitude = Longitude;
        this.Latitude = Latitude;
    }

    public SphericalCoordinate() { }

    public float getLongitude() { return Longitude; }
    public float getLatitude() { return Latitude; }

    public void setLongitude(float Longitude) { this.Longitude = Longitude; }
    public void setLatitude(float Latitude) { this.Latitude = Latitude; }

    public override bool Equals(object obj)
    {
        if(obj == null || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            SphericalCoordinate sphericalCoordinate = (SphericalCoordinate)obj;
            return (this.Longitude == sphericalCoordinate.getLongitude()) 
                    && (this.Latitude == sphericalCoordinate.getLatitude());
        }
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}