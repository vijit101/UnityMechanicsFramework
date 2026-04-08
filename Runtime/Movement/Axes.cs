namespace GameplayMechanicsUMFOSS.Movement {
  public class Axes {
    public float X;
    public float Y;
    public float Z;

    public Axes(float x, float y, float z) {
      this.X = x;
      this.Y = y;
      this.Z = z;
    }

    public Axes(float x, float y) {
      this.X = x;
      this.Y = y;
      this.Z = 0f;
    }
  }
}
