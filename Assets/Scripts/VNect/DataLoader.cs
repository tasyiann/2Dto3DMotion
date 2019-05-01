using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FileHelpers;
using System;
using System.IO;
using System.Globalization;
using static AnimationFilesHacker.VNectSkeleton;

namespace AnimationFilesHacker
{
    public class DataLoader
    {
        public string FileName { get; set; }


        [DelimitedRecord(","), IgnoreFirst(1)]
        public class DelimiterComma
        {
            public int frameNum;
            public float[] nums;
        }

        [FixedLengthRecord(), IgnoreFirst(1)]
        public class DelimiterTab
        {
            [FieldFixedLength(4)]
            public int frameNum;
            [FieldFixedLength(11), FieldArrayLength(33)]
            public float[] nums;
        }
        private Delimiter delim;

        public enum DataType
        {
            Raw2D, Raw3D, Ik3D, Ik2D, UCY_Ik3D
        }

        public DataLoader(string filename)
        {
            FileName = filename;
        }

        public enum Delimiter
        {
            Comma, Tab
        }

        public List<AnimationFrame> getAllFrames(DataType typeOfData)
        {
            setParams(typeOfData);
            List<AnimationFrame> frames = new List<AnimationFrame>();


            if ((int)delim == (int)Delimiter.Comma)
            {
                var engine = new FileHelperEngine<DelimiterComma>();
                DelimiterComma[] result = engine.ReadFile(Path.GetFullPath(FileName));
                for (int frameIndex = 0; frameIndex < result.Length; frameIndex++)
                {
                    DelimiterComma record = result[frameIndex];
                    List<Vector3> joints = new List<Vector3>();
                    for (int numIndex = 0; numIndex < record.nums.Length; numIndex += dimensions)
                    {
                        Vector3 joint = new Vector3(record.nums[numIndex], record.nums[numIndex + 1], dimensions==3?record.nums[numIndex + 2]:0f);
                        joints.Add(joint);
                    }
                    // Calculate the root, if it doesn't exist in file (14th joint).
                    if ( (record.nums.Length / dimensions) == (int)JointsDefinition.Root)
                    {
                        joints.Add((joints[(int)JointsDefinition.LeftUpLeg] + joints[(int)JointsDefinition.RightUpLeg]) / 2f);
                    }
                    frames.Add(new AnimationFrame(joints, record.frameNum));
                }
            }

            return frames;
        }

  
        private int dimensions;
        private void setParams(DataType typeOfData)
        {
            switch (typeOfData)
            {
                case DataType.Raw2D:
                    delim = Delimiter.Comma;
                    dimensions = 2;
                    break;
                case DataType.Ik2D:
                    delim = Delimiter.Comma;
                    dimensions = 2;
                    break;
                case DataType.Ik3D:
                    delim = Delimiter.Comma;
                    dimensions = 3;
                    break;
                case DataType.Raw3D:
                    delim = Delimiter.Comma;
                    dimensions = 3;
                    break;
                case DataType.UCY_Ik3D:
                    delim = Delimiter.Comma;
                    dimensions = 3;
                    break;
            }
        }


        /*
  public List<VNectFrame> getMotionFrames()
  {
      List<VNectFrame> frames = new List<VNectFrame>();
      string fileName = DirPath + @"\" + getFileName(DataType.Motion);
      StreamReader sr = File.OpenText(fileName);
      string tuple = String.Empty;
      bool isFirstLineRead = false;
      while ((tuple = sr.ReadLine()) != null)
      {
          // Skip first line
          if (!isFirstLineRead)
          {
              tuple = sr.ReadLine();
              isFirstLineRead = true;
              continue;
          }
          List<Vector3> joints = new List<Vector3>();
          tuple = System.Text.RegularExpressions.Regex.Replace(tuple, @"\s+", " ");
          string[] array = tuple.Split(' ');
          Debug.Log("LENGHT:" + array.Length);
          try
          {
              int frameNum, indxOfFrame;
              if (array[0].CompareTo("")==0)
                  indxOfFrame = 1;
              else
                  indxOfFrame = 0;

              frameNum = int.Parse(array[indxOfFrame]);

              for (int i = indxOfFrame+1; i < indxOfFrame + 33; i += 3)
              {
                  try
                  {
                      joints.Add(new Vector3(float.Parse(array[i], CultureInfo.InvariantCulture),
                      float.Parse(array[i + 1], CultureInfo.InvariantCulture),
                      float.Parse(array[i + 2], CultureInfo.InvariantCulture)));

                  }
                  catch (Exception e)
                  {
                      Debug.LogError(tuple + "\n\n\n" + array[i] + " " + array[i + 2] + " " + array[i + 3]);
                  }

              }
              frames.Add(new VNectFrame(joints, frameNum));
          }catch(Exception e)
          {
              Debug.LogError("First word: "+ array[0] + "Second: "+array[1]+"\nwhole:"+tuple);

          }
      }
      return frames;
  }
  */



    }



}


