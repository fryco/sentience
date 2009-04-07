/*
    A buffer containing a number of metagrids
    Copyright (C) 2009 Bob Mottram
    fuzzgun@gmail.com

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using sluggish.utilities;

namespace sentience.core
{
	public class metagridBuffer
	{
		// buffer containing grids
		public metagrid[] buffer;
		
		// index number of the currently active grid
		protected int current_buffer_index;
		
		protected List<float> grid_centres;
		protected int current_grid_index;
		
		protected float dimension_mm;
		
		protected List<float> localisations;
		
        #region "constructor"
		
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="no_of_grids">The number of sub grids</param>
        /// <param name="grid_type">the type of sub grids</param>
        /// <param name="dimension_mm">dimension of the smallest sub grid</param>
        /// <param name="dimension_vertical_mm">vertical dimension of the smallest sub grid</param>
        /// <param name="cellSize_mm">cell size of the smallest sub grid</param>
        /// <param name="localisationRadius_mm">localisation radius within the smallest sub grid</param>
        /// <param name="maxMappingRange_mm">maximum mapping radius within the smallest sub grid</param>
        /// <param name="vacancyWeighting">vacancy model weighting, typically between 0.2 and 2</param>
        public metagridBuffer(
            int no_of_grids,
            int grid_type,
		    int dimension_mm, 
            int dimension_vertical_mm, 
            int cellSize_mm, 
            int localisationRadius_mm, 
            int maxMappingRange_mm, 
            float vacancyWeighting)
        {
            this.dimension_mm = dimension_mm;
        
			// create the buffer
			buffer = new metagrid[2];
			for (int i = 0; i < 2; i++)
			{
				buffer[i] = 
					new metagrid(
				        no_of_grids,
				        grid_type,
				        dimension_mm,
				        dimension_vertical_mm,
				        cellSize_mm,
				        localisationRadius_mm,
				        maxMappingRange_mm,
				        vacancyWeighting);
			}
			current_buffer_index = 0;
			current_grid_index = 0;
			grid_centres = new List<float>();
			localisations = new List<float>();
		}
		
        #endregion
		
		#region "resetting"
		
		public void Reset()
		{
		    for (int i = 0; i < 2; i++) buffer[i].Clear();
		    grid_centres.Clear();
		    localisations.Clear();
		    current_buffer_index = 0;
		    current_grid_index = 0;
		}
		
		#endregion
		
		#region "loading a path"
		
		List<float> path;
		
        /// <summary>
        /// loads two encoder traces and contructs a path
        /// </summary>
        /// <param name="path_filename"></param>
        public void LoadPath(
            string path_filename)
        {
            if (File.Exists(path_filename))
            {
                float half_dimension_sqr = dimension_mm * 0.5f;
                half_dimension_sqr *= half_dimension_sqr;
            
                Reset();
                path = new List<float>();
                FileStream fs = fs = File.Open(path_filename, FileMode.Open);
                BinaryReader br = new BinaryReader(fs);

                int entries = br.ReadInt32();
                float last_x=0, last_y=0, last_z=0;
                for (int t = 0; t < entries; t++)
                {
                    OdometryData data = OdometryData.Read(br);
                    path.Add(data.x);
                    path.Add(data.y);
                    
                    if (t == 0)
                    {
                        last_x = data.x;
                        last_y = data.y;
                        grid_centres.Add(data.x);
                        grid_centres.Add(data.y);
                        grid_centres.Add(0.0f);
                    }
                    else
                    {
                        float dx = data.x - last_x;
                        float dy = data.y - last_y;
                        float dist_sqr = dx*dx + dy*dy;
                        if (dist_sqr >= half_dimension_sqr)
                        {
                            last_x = data.x;
                            last_y = data.y;
                            grid_centres.Add(data.x);
                            grid_centres.Add(data.y);
                            grid_centres.Add(0.0f);
                        }
                    }
                }

                br.Close();
                fs.Close();
                
	            // position the first two grids
	            if (grid_centres.Count >= 3)
	            {
	                SetPosition(grid_centres[0], grid_centres[1], grid_centres[2]);
	                SetNextPosition(grid_centres[0], grid_centres[1], grid_centres[2]);
	            }
	            if (grid_centres.Count >= 6)
	            {
	                SetNextPosition(grid_centres[3], grid_centres[4], grid_centres[5]);
	            }
                
                // save path data for debugging purposes
                int img_width = 640;
                int img_height = 480;
                byte[] img = new byte[img_width * img_height * 3];
                Bitmap bmp = new Bitmap(img_width, img_height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                string filename = "debug_path_grids.jpg";
                ShowPath(img, img_width, img_height, true, false);
                BitmapArrayConversions.updatebitmap_unsafe(img, bmp);
                bmp.Save(filename, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            else
            {
                Console.WriteLine("path file " + path_filename + " not found");
            }            
        }
        
        public void ShowPath(
            byte[] img, 
            int img_width, 
            int img_height,
            bool show_grids,
            bool show_localisations)
        {
            // clear the image
            for (int i = (img_width*img_height*3)-1; i >= 0; i--) img[i] = 255;
            
            // find the bounding box
            float tx = float.MaxValue;
            float bx = float.MinValue;
            float ty = float.MaxValue;
            float by = float.MinValue;
            for (int  i = 0; i < path.Count; i += 3)
            {
                float x = path[i];
                float y = path[i+1];
                if (x < tx) tx = x;
                if (x > bx) bx = x;
                if (y < ty) ty = y;
                if (y > by) by = y;
            }
            
            // enlarge the bounding box
            float w = bx - tx;
            tx -= (w * 0.1f);
            bx += (w * 0.1f);
            float h = by - ty;
            ty -= (h * 0.1f);
            by += (h * 0.1f);

            // show the path                        
            int prev_x = 0;
            int prev_y = 0;
            for (int  i = 0; i < path.Count; i += 3)
            {
                int x = (int)((path[i] - tx) * img_width / (bx - tx));
                int y = img_height - 1 - (int)((path[i+1] - ty) * img_height / (by - ty));
                if (i > 0)
                    drawing.drawLine(img, img_width, img_height, prev_x, prev_y, x, y, 0,0,0, 0,false);
                prev_x = x;
                prev_y = y;
            }
            
            // show grids along the path
            if (show_grids)
            {
	            int radius = (int)((dimension_mm/2) * img_width / (bx - tx));
	            for (int i = 0; i < grid_centres.Count; i += 3)
	            {
	                int x = (int)((grid_centres[i] - tx) * img_width / (bx - tx));
	                int y = img_height - 1 - (int)((grid_centres[i + 1] - ty) * img_height / (by - ty));
	                drawing.drawLine(img, img_width, img_height, x - radius, y - radius, x + radius, y - radius, 0,255,0, 0, false);
	                drawing.drawLine(img, img_width, img_height, x - radius, y - radius, x - radius, y + radius, 0,255,0, 0, false);
	                drawing.drawLine(img, img_width, img_height, x - radius, y + radius, x + radius, y + radius, 0,255,0, 0, false);
	                drawing.drawLine(img, img_width, img_height, x + radius, y - radius, x + radius, y + radius, 0,255,0, 0, false);
	            }
            }
            
            // show localisations
            if (show_localisations)
            {
                int radius = (int)(200 * img_width / (bx - tx));
	            for (int i = 0; i < localisations.Count; i += 4)
	            {
	                int x = (int)((localisations[i] - tx) * img_width / (bx - tx));
	                int y = img_height - 1 - (int)((localisations[i + 1] - ty) * img_height / (by - ty));
	                float pan = localisations[i + 3] + (float)Math.PI;
	                int x2 = x + (int)(radius * Math.Sin(pan));
	                int y2 = y + (int)(radius * Math.Sin(pan));
	                drawing.drawLine(img, img_width, img_height, x,y,x2,y2,0,255,0, 0,false);
	                drawing.drawSpot(img, img_width, img_height, x,y,2, 0,255,0);	                
	            }
            }
        }
		
		#endregion
		
        #region "setting the centre position of the grid"
		
		/// <summary>
		/// sets the centre position for the given grid buffer
		/// </summary>
		/// <param name="buffer_index">index number of the buffer</param>
		/// <param name="centre_x_mm">x coordinate in millimetres</param>
		/// <param name="centre_y_mm">y coordinate in millimetres</param>
		/// <param name="centre_z_mm">z coordinate in millimetres</param>
		protected void SetPosition(
		    int buffer_index,
		    float centre_x_mm,
		    float centre_y_mm,
		    float centre_z_mm)
		{
			buffer[buffer_index].SetPosition(centre_x_mm, centre_y_mm, centre_z_mm, 0.0f);
		}

		/// <summary>
		/// sets the centre position for the current grid
		/// </summary>
		/// <param name="centre_x_mm">x coordinate in millimetres</param>
		/// <param name="centre_y_mm">y coordinate in millimetres</param>
		/// <param name="centre_z_mm">z coordinate in millimetres</param>
		protected void SetPosition(
		    float centre_x_mm,
		    float centre_y_mm,
		    float centre_z_mm)
		{
			SetPosition(current_buffer_index, centre_x_mm, centre_y_mm, centre_z_mm);
		}

		/// <summary>
		/// sets the centre position for the next grid which will be entered
		/// </summary>
		/// <param name="centre_x_mm">x coordinate in millimetres</param>
		/// <param name="centre_y_mm">y coordinate in millimetres</param>
		/// <param name="centre_z_mm">z coordinate in millimetres</param>
		protected void SetNextPosition(
		    float centre_x_mm,
		    float centre_y_mm,
		    float centre_z_mm)
		{
			SetPosition(1 - current_buffer_index, centre_x_mm, centre_y_mm, centre_z_mm);
		}
		
        #endregion
		
        #region "mapping and localising"

        /// <summary>
        /// Mapping
        /// </summary>
        /// <param name="body_width_mm">width of the robot body in millimetres</param>
        /// <param name="body_length_mm">length of the robot body in millimetres</param>
        /// <param name="body_centre_of_rotation_x">x centre of rotation of the robot, relative to the top left corner</param>
        /// <param name="body_centre_of_rotation_y">y centre of rotation of the robot, relative to the top left corner</param>
        /// <param name="body_centre_of_rotation_z">z centre of rotation of the robot, relative to the top left corner</param>
        /// <param name="head_centroid_x">head centroid x position in millimetres relative to the top left corner of the body</param>
        /// <param name="head_centroid_y">head centroid y position in millimetres relative to the top left corner of the body</param>
        /// <param name="head_centroid_z">head centroid z position in millimetres relative to the top left corner of the body</param>
        /// <param name="head_pan">head pan angle in radians</param>
        /// <param name="head_tilt">head tilt angle in radians</param>
        /// <param name="head_roll">head roll angle in radians</param>
        /// <param name="baseline_mm">stereo camera baseline in millimetres</param>
        /// <param name="stereo_camera_position_x">stereo camera x position in millimetres relative to the head centroid</param>
        /// <param name="stereo_camera_position_y">stereo camera y position in millimetres relative to the head centroid</param>
        /// <param name="stereo_camera_position_z">stereo camera z position in millimetres relative to the head centroid</param>
        /// <param name="stereo_camera_pan">stereo camera pan in radians relative to the head</param>
        /// <param name="stereo_camera_tilt">stereo camera tilt in radians relative to the head</param>
        /// <param name="stereo_camera_roll">stereo camera roll in radians relative to the head</param>
        /// <param name="image_width">image width for each stereo camera</param>
        /// <param name="image_height">image height for each stereo camera</param>
        /// <param name="FOV_degrees">field of view for each stereo camera in degrees</param>
        /// <param name="stereo_features">stereo features (disparities) for each stereo camera</param>
        /// <param name="stereo_features_colour">stereo feature colours for each stereo camera</param>
        /// <param name="stereo_features_uncertainties">stereo feature uncertainties (priors) for each stereo camera</param>
        /// <param name="sensormodel">sensor model for each stereo camera</param>
        /// <param name="robot_pose">current estimated position and orientation of the robots centre of rotation</param>
        public void Map(
		    float body_width_mm,
		    float body_length_mm,
		    float body_centre_of_rotation_x,
		    float body_centre_of_rotation_y,
		    float body_centre_of_rotation_z,
		    float head_centroid_x,
		    float head_centroid_y,
		    float head_centroid_z,
		    float head_pan,
		    float head_tilt,
		    float head_roll,
		    float[] baseline_mm,
		    float[] stereo_camera_position_x,
		    float[] stereo_camera_position_y,
		    float[] stereo_camera_position_z,
		    float[] stereo_camera_pan,
		    float[] stereo_camera_tilt,
		    float[] stereo_camera_roll,
            int[] image_width,
            int[] image_height,
            float[] FOV_degrees,
		    float[][] stereo_features,
		    byte[][,] stereo_features_colour,
		    float[][] stereo_features_uncertainties,
            stereoModel[] sensormodel,
            pos3D robot_pose)
        {        
		    Parallel.For(0, 2, delegate(int i)
		    {			
                pos3D[] left_camera_location = null;
                pos3D[] right_camera_location = null;
                
                buffer[i].Map(
		            body_width_mm,
		            body_length_mm,
		            body_centre_of_rotation_x,
		            body_centre_of_rotation_y,
		            body_centre_of_rotation_z,
		            head_centroid_x,
		            head_centroid_y,
		            head_centroid_z,
		            head_pan,
		            head_tilt,
		            head_roll,
		            baseline_mm,
		            stereo_camera_position_x,
		            stereo_camera_position_y,
		            stereo_camera_position_z,
		            stereo_camera_pan,
		            stereo_camera_tilt,
		            stereo_camera_roll,
                    image_width,
                    image_height,
                    FOV_degrees,
		            stereo_features,
		            stereo_features_colour,
		            stereo_features_uncertainties,
                    sensormodel,
                    ref left_camera_location,
                    ref right_camera_location,
                    robot_pose);
            });
        }

        /// <summary>
        /// Localisation
        /// </summary>
        /// <param name="body_width_mm">width of the robot body in millimetres</param>
        /// <param name="body_length_mm">length of the robot body in millimetres</param>
        /// <param name="body_centre_of_rotation_x">x centre of rotation of the robot, relative to the top left corner</param>
        /// <param name="body_centre_of_rotation_y">y centre of rotation of the robot, relative to the top left corner</param>
        /// <param name="body_centre_of_rotation_z">z centre of rotation of the robot, relative to the top left corner</param>
        /// <param name="head_centroid_x">head centroid x position in millimetres relative to the top left corner of the body</param>
        /// <param name="head_centroid_y">head centroid y position in millimetres relative to the top left corner of the body</param>
        /// <param name="head_centroid_z">head centroid z position in millimetres relative to the top left corner of the body</param>
        /// <param name="head_pan">head pan angle in radians</param>
        /// <param name="head_tilt">head tilt angle in radians</param>
        /// <param name="head_roll">head roll angle in radians</param>
        /// <param name="baseline_mm">stereo camera baseline in millimetres</param>
        /// <param name="stereo_camera_position_x">stereo camera x position in millimetres relative to the head centroid</param>
        /// <param name="stereo_camera_position_y">stereo camera y position in millimetres relative to the head centroid</param>
        /// <param name="stereo_camera_position_z">stereo camera z position in millimetres relative to the head centroid</param>
        /// <param name="stereo_camera_pan">stereo camera pan in radians relative to the head</param>
        /// <param name="stereo_camera_tilt">stereo camera tilt in radians relative to the head</param>
        /// <param name="stereo_camera_roll">stereo camera roll in radians relative to the head</param>
        /// <param name="image_width">image width for each stereo camera</param>
        /// <param name="image_height">image height for each stereo camera</param>
        /// <param name="FOV_degrees">field of view for each stereo camera in degrees</param>
        /// <param name="stereo_features">stereo features (disparities) for each stereo camera</param>
        /// <param name="stereo_features_colour">stereo feature colours for each stereo camera</param>
        /// <param name="stereo_features_uncertainties">stereo feature uncertainties (priors) for each stereo camera</param>
        /// <param name="sensormodel">sensor model for each stereo camera</param>
        /// <param name="left_camera_location">returned position and orientation of the left camera on each stereo camera</param>
        /// <param name="right_camera_location">returned position and orientation of the right camera on each stereo camera</param>
        /// <param name="no_of_samples">number of sample poses</param>
        /// <param name="sampling_radius_major_mm">major radius for samples, in the direction of robot movement</param>
        /// <param name="sampling_radius_minor_mm">minor radius for samples, perpendicular to the direction of robot movement</param>
        /// <param name="robot_pose">current estimated position and orientation of the robots centre of rotation</param>
        /// <param name="max_orientation_variance">maximum variance in orientation in radians, used to create sample poses</param>
        /// <param name="max_tilt_variance">maximum variance in tilt angle in radians, used to create sample poses</param>
        /// <param name="max_roll_variance">maximum variance in roll angle in radians, used to create sample poses</param>
        /// <param name="poses">list of poses tried</param>
        /// <param name="pose_score">list of pose matching scores</param>
        /// <param name="pose_offset">offset of the best pose from the current one</param>
		/// <param name="rnd">random number generator</param>
        /// <param name="buffer_transition">have we transitioned to the next grid buffer?</param>
        /// <returns>best localisation matching score</returns>
        public float Localise(
		    float body_width_mm,
		    float body_length_mm,
		    float body_centre_of_rotation_x,
		    float body_centre_of_rotation_y,
		    float body_centre_of_rotation_z,
		    float head_centroid_x,
		    float head_centroid_y,
		    float head_centroid_z,
		    float head_pan,
		    float head_tilt,
		    float head_roll,
		    float[] baseline_mm,
		    float[] stereo_camera_position_x,
		    float[] stereo_camera_position_y,
		    float[] stereo_camera_position_z,
		    float[] stereo_camera_pan,
		    float[] stereo_camera_tilt,
		    float[] stereo_camera_roll,
            int[] image_width,
            int[] image_height,
            float[] FOV_degrees,
		    float[][] stereo_features,
		    byte[][,] stereo_features_colour,
		    float[][] stereo_features_uncertainties,
            stereoModel[] sensormodel,
            ref pos3D[] left_camera_location,
            ref pos3D[] right_camera_location,
            int no_of_samples,
            float sampling_radius_major_mm,
            float sampling_radius_minor_mm,
            pos3D robot_pose,
            float max_orientation_variance,
            float max_tilt_variance,
            float max_roll_variance,
            List<pos3D> poses,
            List<float> pose_score,
		    Random rnd,
            ref pos3D pose_offset,
            ref bool buffer_transition)
        {
            buffer_transition = false;
        
            // distance to the centre of the currently active grid
            float dx = robot_pose.x - buffer[current_buffer_index].x;
            float dy = robot_pose.y - buffer[current_buffer_index].y;
            float dz = robot_pose.z - buffer[current_buffer_index].z;
            float dist_to_grid_centre_sqr_0 = dx*dx + dy*dy + dz*dz;
            dx = robot_pose.x - buffer[1 - current_buffer_index].x;
            dy = robot_pose.y - buffer[1 - current_buffer_index].y;
            dz = robot_pose.z - buffer[1 - current_buffer_index].z;
            float dist_to_grid_centre_sqr_1 = dx*dx + dy*dy + dz*dz;
            
            // if we are closer to the next grid than the current one
            // then swap the currently active grid
            if (dist_to_grid_centre_sqr_1 < dist_to_grid_centre_sqr_0)
            {
                current_buffer_index = 1 - current_buffer_index;
                if (current_grid_index < (grid_centres.Count/3) - 1)
                {
                    buffer[1 - current_buffer_index].Clear();
                    
                    // move into the next grid
                    current_grid_index++;
                    
                    // set the next grid centre                                    
                    SetNextPosition(
                        grid_centres[((current_grid_index+1) * 3)],
                        grid_centres[((current_grid_index+1) * 3)] + 1,
                        grid_centres[((current_grid_index+1) * 3)] + 2);
                }
                buffer_transition = true;
            }
        
            float matching_score = 
	            buffer[current_buffer_index].Localise(
			        body_width_mm,
			        body_length_mm,
			        body_centre_of_rotation_x,
			        body_centre_of_rotation_y,
			        body_centre_of_rotation_z,
			        head_centroid_x,
			        head_centroid_y,
			        head_centroid_z,
			        head_pan,
			        head_tilt,
			        head_roll,
			        baseline_mm,
			        stereo_camera_position_x,
			        stereo_camera_position_y,
			        stereo_camera_position_z,
			        stereo_camera_pan,
			        stereo_camera_tilt,
			        stereo_camera_roll,
	                image_width,
	                image_height,
	                FOV_degrees,
			        stereo_features,
			        stereo_features_colour,
			        stereo_features_uncertainties,
	                sensormodel,
	                ref left_camera_location,
	                ref right_camera_location,
	                no_of_samples,
	                sampling_radius_major_mm,
	                sampling_radius_minor_mm,
	                robot_pose,
	                max_orientation_variance,
	                max_tilt_variance,
	                max_roll_variance,
	                poses,
	                pose_score,
			        rnd,
	                ref pose_offset);
	        
	        // add this to the list of localisations                
	        localisations.Add(robot_pose.x + pose_offset.x);
	        localisations.Add(robot_pose.y + pose_offset.y);
	        localisations.Add(robot_pose.z + pose_offset.z);
	        localisations.Add(pose_offset.pan);
	        localisations.Add(matching_score);
	        
	        return(matching_score);
        }
		
        #endregion
		
	}
}