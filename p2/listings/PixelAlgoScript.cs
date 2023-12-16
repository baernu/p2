
using UnityEngine;
using UnityEngine.UI;

using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;

public class PixelAlgoScript : MonoBehaviour
{
    public RenderTexture renderTexture;
    public RawImage rawImage;
    Texture2D texture;
    Texture2D rendTex;
    public Transform p0;
    public Transform p1;
    public Transform p2;
    public Transform p3;
    public Transform p5;
    private Vector3 r0;
    private Vector3 r1;
    private Vector3 r2;
    private Vector3 r3;
    private Vector3 r5;
    private Vector3[] srcPolygon;
    private Vector3[] dstRect;
    public Camera renderCamera;
    public float screen_height;
    public float screen_width;

    private void Start()
    {
        texture = new Texture2D(800, 450, TextureFormat.RGBA32, false);
        rendTex = new Texture2D(800, 450, TextureFormat.RGBA32, false);
        srcPolygon = new Vector3[4];
        dstRect = new Vector3[4]
        {
            new Vector3(0, 0, 0),
            new Vector3(texture.width, 0, 0),
            new Vector3(texture.width, texture.height, 0),
            new Vector3(0, texture.height, 0)
        };


        r0 = p0.position;
        r1 = p1.position;
        r2 = p2.position;
        r3 = p3.position;
        r5 = p5.position;

    }

    
    

    void Update()
    {
        //adjustFov();
        calculateEdgeScreenPoints();
        //doCalculateTransformVertices();


        rendTex = toTexture2D(renderTexture);
   
        
        Mat inputMat = new Mat(rendTex.height, rendTex.width, CvType.CV_8UC4);
        Mat outputMat = new Mat(texture.height, texture.width, CvType.CV_8UC4);

        OpenCVForUnity.UnityUtils.Utils.texture2DToMat(rendTex, inputMat, true, 0);


        Mat src_mat = new Mat(4, 1, CvType.CV_32FC2);
        Mat dst_mat = new Mat(4, 1, CvType.CV_32FC2);
        src_mat.put(0, 0, srcPolygon[0].x, srcPolygon[0].y, srcPolygon[1].x, srcPolygon[1].y, srcPolygon[3].x, srcPolygon[3].y, srcPolygon[2].x, srcPolygon[2].y);
        dst_mat.put(0, 0, 0.0, 0.0, outputMat.cols(), 0.0, 0.0, outputMat.rows(), outputMat.cols(), outputMat.rows());

        Mat perspectiveTransform = Imgproc.getPerspectiveTransform(src_mat, dst_mat);
        Imgproc.warpPerspective(inputMat, outputMat, perspectiveTransform, new Size(outputMat.cols(), outputMat.rows()));


        Texture2D outputTexture = new Texture2D(outputMat.cols(), outputMat.rows(), TextureFormat.RGBA32, false);

        OpenCVForUnity.UnityUtils.Utils.matToTexture2D(outputMat, outputTexture);
        
        for (int y = 0; y < outputTexture.height; y++)
        {
            for (int x = 0; x < outputTexture.width; x++)
            {
                Color color = outputTexture.GetPixel(x, y);
                texture.SetPixel(x, y, color);
            }
        }


        texture.Apply();

        rawImage.material.SetTexture("_MainTex", texture);
    }

    void calculateEdgeScreenPoints()
    {

        Vector3 w0 = renderCamera.ViewportToWorldPoint(new Vector3(0, 0, -renderCamera.transform.position.z));
        Vector3 w1 = renderCamera.ViewportToWorldPoint(new Vector3(0, 1, -renderCamera.transform.position.z));
        Vector3 w2 = renderCamera.ViewportToWorldPoint(new Vector3(1, 1, -renderCamera.transform.position.z));
        Vector3 w3 = renderCamera.ViewportToWorldPoint(new Vector3(1, 0, -renderCamera.transform.position.z));
        

        Vector3 cam = renderCamera.transform.position;
        Vector3 dir_screen_vec = p5.transform.position - cam;

        float distance = dir_screen_vec.z / dir_screen_vec.normalized.z;
        float deltaX = distance * Mathf.Tan(renderCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float length = Mathf.Sqrt(deltaX * deltaX + distance * distance);
        Debug.Log("deletaX: " + deltaX);
        Debug.Log("distance: " + distance);
        Debug.Log("length: " + length);
        Debug.Log("dir vector: " + dir_screen_vec);

        //Debug.Log("vec q0 : " + Quaternion.Euler(-45, -45, 0) * dir_screen_vec);

        Vector3 q1 = Quaternion.Euler(-renderCamera.fieldOfView * 0.5f, -renderCamera.fieldOfView * 0.5f, 0) * cloneDirVector(dir_screen_vec);
        Vector3 q0 = Quaternion.Euler(renderCamera.fieldOfView * 0.5f, -renderCamera.fieldOfView * 0.5f, 0) * cloneDirVector(dir_screen_vec);
        Vector3 q3 = Quaternion.Euler(renderCamera.fieldOfView * 0.5f, renderCamera.fieldOfView * 0.5f, 0) * cloneDirVector(dir_screen_vec);
        Vector3 q2 = Quaternion.Euler(-renderCamera.fieldOfView * 0.5f, renderCamera.fieldOfView * 0.5f, 0) * cloneDirVector(dir_screen_vec);

        //Debug.Log("Vectors Vieport are:  " + q0 + " " + q1 + " " + q2 + " " + q3 + " " + p5.transform.position);

        q0 = q0.normalized * length;
        q1 = q1.normalized * length;
        q2 = q2.normalized * length;
        q3 = q3.normalized * length;

        q0 = cam + q0;
        q1 = cam + q1;
        q2 = cam + q2;
        q3 = cam + q3;



        //Debug.Log("Vectors Screen Edges are:  "  + r0 + " " + r1 + " " + r2 + " " + r3 + " " + r5);


        Vector3 v = w3 - w0;
        Vector3 u = w1 - w0;


        Vector3 n;

        Vector3 qn = cloneDirVector(dir_screen_vec);
        float dp = distance;
        qn = qn.normalized;


        n = qn;

        Vector3 pr0 = -cam + r0;
        Vector3 pr1 = -cam + r1;
        Vector3 pr2 = -cam + r2;
        Vector3 pr3 = -cam + r3;

        //Debug.Log(" d1 - d3 is: " + d1 + " " + d2 + " " + d3);
        //d is not for all points the same
        //i want the vector cam + (-cam + r0) * d is point on the plane
        //float d = q0.x * n.x + q0.y * n.y + q0.z * n.z;
        float d = w0.x * n.x + w0.y * n.y + w0.z * n.z;
        float t0 = (d - n.x * cam.x - n.y * cam.y - n.z * cam.z) / (pr0.x * n.x + pr0.y * n.y + pr0.z * n.z);
        float t1 = (d - n.x * cam.x - n.y * cam.y - n.z * cam.z) / (pr1.x * n.x + pr1.y * n.y + pr1.z * n.z);
        float t2 = (d - n.x * cam.x - n.y * cam.y - n.z * cam.z) / (pr2.x * n.x + pr2.y * n.y + pr2.z * n.z);
        float t3 = (d - n.x * cam.x - n.y * cam.y - n.z * cam.z) / (pr3.x * n.x + pr3.y * n.y + pr3.z * n.z);

        Debug.Log("t0 - t3: " + t0 + " ," + t1 + " ," + t2 + " ," + t3);
        Debug.Log("u, v, n: " + u + " ," + v + " ," + n);

        Debug.Log("cam: " + cam);
        Debug.Log("n * d + q5: " + n * d + p5.transform.position);

        pr0 = pr0 * t0 + cam;
        pr1 = pr1 * t1 + cam;
        pr2 = pr2 * t2 + cam;
        pr3 = pr3 * t3 + cam;
        Debug.Log("Vectors Vieport are:  " + q0 + " " + q1 + " " + q2 + " " + q3 + " " + p5.transform.position);
        Debug.Log("Vectors Vieport w0 - w3 are:  " + w0 + " " + w1 + " " + w2 + " " + w3 + " " + p5.transform.position);
        Debug.Log("Points in the terrain: (" + pr0 + ") ," + "( " + pr1 + ") ," + "( " + pr2 + ") ," + "( " + pr3 + ")");

        
        /*
        float y0 = (pr0.y - w0.y - u.y * (pr0.x - w0.x) / v.x) / (u.y * (1 - u.x / v.x));
        float x0 = (pr0.x - w0.x - y0 * u.x) / v.x;
        float y1 = (pr1.y - w0.y - u.y * (pr1.x - w0.x) / v.x) / (u.y * (1 - u.x / v.x));
        float x1 = (pr1.x - w0.x - y1 * u.x) / v.x;
        float y2 = (pr2.y - w0.y - u.y * (pr2.x - w0.x) / v.x) / (u.y * (1 - u.x / v.x));
        float x2 = (pr2.x - w0.x - y2 * u.x) / v.x;
        float y3 = (pr3.y - w0.y - u.y * (pr3.x - w0.x) / v.x) / (u.y * (1 - u.x / v.x));
        float x3 = (pr3.x - w0.x - y3 * u.x) / v.x;
        */
        
        float y0 = (pr0.y - w0.y) / u.y;
        float x0 = (pr0.x - w0.x - y0*u.x) / v.x;
        //x0 = (pr0.x - q2.x -y0 * u.x) / v.x;
        //y0 = (pr0.y - q2.y -x0 * v.x) / u.y;

        
        float y1 = (pr1.y - w0.y) / u.y;
        float x1 = (pr1.x - w0.x -y1*u.x) / v.x;
        //x1 = (pr1.x - q2.x - y1 * u.x) / v.x;
        //y1 = (pr1.y - q2.y - x1 * v.x) / u.y;

        
        float y2 = (pr2.y - w0.y) / u.y;
        float x2 = (pr2.x - w0.x -y2*u.x) / v.x;
        //x2 = (pr2.x - q2.x - y2 * u.x) / v.x;
        //y2 = (pr2.y - q2.y - x2 * v.x) / u.y;

        
        float y3 = (pr3.y - w0.y) / u.y;
        float x3 = (pr3.x - w0.x -y3*u.x) / v.x;
        //x3 = (pr3.x - q2.x - y3 * u.x) / v.x;
        //y3 = (pr3.y - q2.y - x3 * v.x) / u.y;

        


        float rx7 = (q3.x - q2.x) / v.x;
        float ry7 = (q3.y - q2.y) / u.y;



        srcPolygon = new Vector3[]
        {
            new Vector3((int)(x0 * renderTexture.width), (int)(y0 * renderTexture.height), 0),
            new Vector3((int)(x1 * renderTexture.width), (int)(y1 * renderTexture.height), 0),
            new Vector3((int)(x2 * renderTexture.width), (int)(y2 * renderTexture.height), 0),
            new Vector3((int)(x3 * renderTexture.width), (int)(y3 * renderTexture.height), 0)
        };



        Debug.Log("Edges of the screen: (" + x0 + "/" + y0 + ") ," + "( " + x1 + "/" + y1 + ") ," + "( " + x2 + "/" + y2 + ") ," + "( " + x3 + "/" + y3);
        Debug.Log("One Edge in canvas is: (" + rx7 + "/" + ry7 + ")");
    }

    Vector3 cloneDirVector(Vector3 dir)
    {
        Vector3 clone = new Vector3();
        clone.x = dir.x;
        clone.y = dir.y;
        clone.z = dir.z;
        return clone;
    }

    Texture2D toTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
        // ReadPixels looks at the active RenderTexture.
        RenderTexture.active = rTex;
        tex.ReadPixels(new UnityEngine.Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        return tex;
    }
    void adjustFov()
    {
        renderCamera.fieldOfView = Mathf.Atan(screen_width / (2 * Mathf.Abs(Camera.main.transform.position.z))) * 360 * 2 / (2 * Mathf.PI) * 1.5f;
    }
    
    Matrix4x4 getViewMatrix_x_ProjectionMatrix()
    {
        return GL.GetGPUProjectionMatrix(renderCamera.projectionMatrix, false) * renderCamera.worldToCameraMatrix;
    }

    void setViewport()
    {
        GL.Viewport(new UnityEngine.Rect(0, 0, Screen.width, Screen.height));
    }

    Vector3[] getMatrixScreenVertices()
    {
        Vector3[] vertices = new Vector3[]
        {
            p0.position,
            p1.position,
            p2.position,
            p3.position
        };
        
        return vertices;
    }

    void doCalculateTransformVertices()
    {
        setViewport();
        Vector3[] vertices = getMatrixScreenVertices();
        Matrix4x4 view_x_projection_matrix = getViewMatrix_x_ProjectionMatrix();
        Matrix4x4 clip_to_viewport_matrix = new Matrix4x4();
        clip_to_viewport_matrix[0, 0] = 0.5f;
        clip_to_viewport_matrix[0, 1] = 0;
        clip_to_viewport_matrix[0, 2] = 0;
        clip_to_viewport_matrix[0, 3] = 0;
        clip_to_viewport_matrix[1, 0] = 0;
        clip_to_viewport_matrix[1, 1] = 0.5f;
        clip_to_viewport_matrix[1, 2] = 0;
        clip_to_viewport_matrix[1, 3] = 0;
        clip_to_viewport_matrix[2, 0] = 0;
        clip_to_viewport_matrix[2, 1] = 0;
        clip_to_viewport_matrix[2, 2] = 1;
        clip_to_viewport_matrix[2, 3] = 0;
        clip_to_viewport_matrix[3, 0] = 1;
        clip_to_viewport_matrix[3, 1] = 1;
        clip_to_viewport_matrix[3, 2] = 0;
        clip_to_viewport_matrix[3, 3] = 1;
        Matrix4x4 result =  view_x_projection_matrix;
        //Matrix4x4 result = view_x_projection_matrix * clip_to_viewport_matrix;
        Vector2[] screenPoints = new Vector2[4];
        for (int i = 0; i < vertices.Length; i++) 
        {
            screenPoints[i].x = vertices[i].x * result[0, 0] + vertices[i].y * result[0, 1] + vertices[i].z * result[0, 2];
            screenPoints[i].y = vertices[i].x * result[1, 0] + vertices[i].y * result[1, 1] + vertices[i].z * result[1, 2];

        }

        Debug.Log("Points of Screen hope to be :" + screenPoints[0].x + "/ " + screenPoints[0].y);
        Debug.Log("Points of Screen hope to be :" + screenPoints[1].x + "/ " + screenPoints[1].y);
        Debug.Log("Points of Screen hope to be :" + screenPoints[2].x + "/ " + screenPoints[2].y);
        Debug.Log("Points of Screen hope to be :" + screenPoints[3].x + "/ " + screenPoints[3].y);


    }



}

