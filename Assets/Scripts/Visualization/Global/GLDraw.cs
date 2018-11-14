using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GLDraw {

    private int[,] jointPairs;
    private int[,] defaultJP = new int[13,2] { {0,1}, {1,2}, {2,3}, {3,4}, {1,5}, {5,6}, {6,7}, {1,8}, {8,9}, {9,10}, {1,11}, {11,12}, {12,13} };
    private Material material;

    public GLDraw(int [,] joint_pairs, Material mat)
    {
        jointPairs = joint_pairs;
        material = mat;
    }

    public GLDraw(Material mat)
    {
        jointPairs = defaultJP;
        material = mat;
    }

    public void drawHorizontalLine(Color color, float y, float length)
    {
        GL.Begin(GL.LINES);
        material.SetPass(0);
        GL.Color(color);
        GL.Vertex(new Vector3(-length/2,y));
        GL.Vertex(new Vector3(length/2, y));
        GL.End();
    }


    public void drawFigure(bool direction,Color color, Vector3[] joints, bool[] available, Vector3 offset, float scaling = 1, float rotateAngle = 0)
    {
        GL.Begin(GL.LINES);
        material.SetPass(0);
        GL.Color(color);
        for (int i=0; i<13; i++)
        {
           
            if( (available == null) || (available!=null && available[jointPairs[i, 0]]==true && available[jointPairs[i, 1]]==true))
            {
                // To tell where the stick figure is looking at
                if (direction && (i==2 || i==3 || i==8 || i==9))
                    GL.Color(Color.magenta);

                GL.Vertex( Quaternion.Euler(0, rotateAngle, 0) * (joints[jointPairs[i, 0]] + offset) * scaling );
                GL.Vertex( Quaternion.Euler(0, rotateAngle, 0) * (joints[jointPairs[i, 1]] + offset) * scaling);
                GL.Color(color);
            }
        }
        GL.End();
    }

    public void drawFigureDebug(bool direction, Color color, Vector3[] joints, bool[] available, Vector3 offset, float scaling = 1, float rotateAngle = 0)
    {
        GL.Begin(GL.LINES);
        material.SetPass(0);
        GL.Color(color);
        for (int i = 0; i < 13; i++)
        {

            if ((available == null) || (available != null && available[jointPairs[i, 0]] == true && available[jointPairs[i, 1]] == true))
            {
                // To tell where the stick figure is looking at
                if (direction && (i == 8 || i==11 || i==2 || i==5) )
                    GL.Color(Color.magenta);
                //if (direction && (i == 9 || i==12) )
                //    GL.Color(Color.green);
                if (!direction && (i == 8 || i == 11 || i == 2 || i == 5))
                    GL.Color(Color.cyan);
                //if (!direction && (i == 9 || i == 12))
                //    GL.Color(Color.yellow);


                GL.Vertex(Quaternion.Euler(0, rotateAngle, 0) * (joints[jointPairs[i, 0]] + offset) * scaling);
                GL.Vertex(Quaternion.Euler(0, rotateAngle, 0) * (joints[jointPairs[i, 1]] + offset) * scaling);
                GL.Color(color);
            }
        }
        GL.End();
    }



    public void drawAxes(Color color, float xStart=-1000, float xEnd=1000, float yStart=-1000, float yEnd=1000)
    {
        // X-axis
        GL.Begin(GL.LINES);
        material.SetPass(0);
        GL.Color(color);
        GL.Vertex(new Vector3(xStart, 0, 0));
        GL.Vertex(new Vector3(xEnd, 0, 0));
        // Y-axes
        GL.Vertex(new Vector3(0, yStart, 0));
        GL.Vertex(new Vector3(0, yEnd, 0));
        GL.End();
    }



    public void drawMotionGraphAllJoints(Neighbour[] path, int jointIndex, int currentFrame)
    {
        // Prepare axes (grid)
        drawAxes(Color.white,0);
        float offset=0;
        for (int i = 0; i < path.Length; i++)
        {
            drawVerticalLine(offset+=0.2f, Color.gray);
        }
        // First 3d, starts at 0.2f
        drawVerticalLine(currentFrame*0.2f + 0.2f, Color.red);

        // Draw motion graph
        drawGraph(jointIndex, 100, path);

    }

    public void drawVerticalLine(float x, Color color)
    {
        GL.Begin(GL.LINES);
        material.SetPass(0);
        GL.Color(color);
        GL.Vertex(new Vector3(x, -1000, 0));
        GL.Vertex(new Vector3(x, 1000, 0));
        GL.End();
    }


    public void drawGraph(int jointIndex, float offset, Neighbour[] path)
    {
        drawMotionGraphJoint(jointIndex, Color.yellow, 'x', path);
        drawMotionGraphJoint(jointIndex, Color.green, 'y', path);
        drawMotionGraphJoint(jointIndex, Color.red, 'z', path);
    }

    public void drawMotionGraphJoint(int jointIndex, Color color, char axis, Neighbour[] path)
    {
        float d = 0.2f;
        GL.Begin(GL.LINE_STRIP);
        material.SetPass(0);
        GL.Color(color);
        float offset = 0;
        float point;
        GL.Vertex(Vector3.zero);

        for (int i = 1; i < path.Length; i++)
        {
            

            if (path != null && path[i] != null && path[i].projection != null)
            {
                switch (axis)
                {
                    case 'x': { point = path[i].projection.joints[jointIndex].x ; break; }
                    case 'y': { point = path[i].projection.joints[jointIndex].y ; break; }
                    case 'z': { point = path[i].projection.joints[jointIndex].z ; break; }
                    default : { point = 0; break; }

                }
                GL.Vertex(new Vector3(offset+=d, point, 0));
            }
        }
        GL.End();
    }

}
