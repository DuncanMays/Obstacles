from math import sin, cos, pi
from neuron import neuron
from random import randint

class agent:

	def __init__(self, DNA, xPos, yPos):
		#this is a boolean representing if the agent is dead or alive, 1 means alive, 0 means dead
		#if this is zero, the agents AI will stop running, effectively killing the agent
		self.alive = bool
		self.alive = 1

		#this is the DNA of the agent, an array which describes all the properties of an agent which can change, effectively defining this agent
		#and allowing it to be recreated elswhere in the program. The array contains, in order:
		#the colour of the agent in RGB, the spread of the agent's eyes, 
		#the 2x3-array representing the default charge of each neuron, and the 3x3x3-array representing the mesh between the two layers of neurons
		#and the outputs
		self.DNA = self.limit(DNA)

		#this holds the list of points which define the shape of the agent (a triangle)
		self.pointList = [(xPos, yPos), (xPos-15, yPos+5), (xPos-15, yPos-5)]

		#these are important enough to exist outside pointlist, since they are used all over the place and referenceing an index in poitslist
		#would be a pain for the processor
		self.xPos = xPos
		self.yPos = yPos

		#these variables will be useful later
		#the speed the agent is moving
		self.vel = 0
		#the angle the agent is at (relative to horizontal)
		self.theta = 0

		#this is the eyes of the organism/agent/whatever, ititialized at its min so the organism doesn't freak out
		self.eyes = [0, 0, 0]
		#this is the angle of spread between the middle eyes and the two outer ones
		self.spread = DNA[1]

		#here's where the fancy AI stuff starts
		#so the brian of the agent is divided into two layers of neurons with inputs and outputs on wither side
		#the inputs are the agent's eyes, which give info to the first layer of three neurons
		#the first layer of neurons then multiplies the data by a certain constant and then passes it on to the second layer
		#the second layer then does the same things and controls the three outputs, one to make the agent move forward and backward,
		#and two to turn left and right

		#this is the collection of neurons which make up the agent's brain
		# 0 is colour 1 is spread, 2 is defaults, 3 is mesh
		#self.brain = [[neuron(0,(0,0,1)),neuron(0,(0,0,0)),neuron(0,(1,0,0))],[neuron(0,(1,0,0)),neuron(1,(0,1,0)),neuron(0,(0,0,1))]]
		self.brain = [[neuron(DNA[2][0][0],DNA[3][1][0]),
						neuron(DNA[2][0][1],DNA[3][1][1]),
						neuron(DNA[2][0][2],DNA[3][1][2])
						],[
						neuron(DNA[2][1][0],DNA[3][2][0]),
						neuron(DNA[2][1][1],DNA[3][2][1]),
						neuron(DNA[2][1][2],DNA[3][2][2])]]

		#this is the mesh which controls how data is passed from the inputs (eyes) to the first layer of neurons
		#this is done because treating eyes like neurons would be an unneccessary amount of overhead
		#self.meshOne = [[1,0,0],[0,1,0],[0,0,1]]
		self.meshOne = DNA[3][0]

	#checks if the agent is within a circle, and kills it if it is, needs to be executed every obstacle, every cycle, called in checkEyes
	def checkDeathByCircle(self, xCircle, yCircle, rad):
		if(pow(self.xPos - xCircle, 2) + pow(self.yPos - yCircle, 2) <= pow(rad, 2)):
			self.die()

	#checks if the agent has left the arena, and kills it if it has, only needs to be executed once per cycle, called in update
	def checkDeathByBoundary(self, bounds):
		if((self.xPos < bounds[0]) | (self.yPos < bounds[1]) | (self.yPos > bounds[2]) | (self.xPos > bounds[3])):
			self.die()

	#returns a value of one to one hundred depending on the closest object to the eye along its path of vision
	#the scale is inverted, so faraway objects have a smaller value but closer objects have a larger value
	#100 means the object is right up against the eye, 0 means the object is further than 100 pixels away
	#this is to make things esier for the neural net, as it would have to invert the signal since closer 
	#objects demand more attention than further away ones, adding to complexity and requiring another neuron
	def checkEyes(self, obstacles, bounds):

		#resets the agents eyes
		self.eyes = [0,0,0]

		#this bit checks for the circular obstacles
		for j in range(0,3):
			for i in obstacles:

				#checks if the agent is in a circle, only needs to be checked once per obstacle, not for every eye
				if(j == 0):
					self.checkDeathByCircle(i[0], i[1], i[2])
			
				#this is the net angle of the ray from the tip of the agent, includng the spread of the eye
				netAngle = self.theta + self.spread*(j - 1)
	
				#this bit checks for intersection with obstacles

				#the function which gives the distance to the closest obstacle is quadratic, meaning its roots could be complex
				#which may cause my computer to blow up. This variable is the descriminant of the quadtratic formula for that function,
				#allowing the program to test for complex roots and ignore them if the appear to avoid self-destruction
				descriminant = pow(cos(netAngle)*(self.xPos - i[0]) + sin(netAngle)*(self.yPos - i[1]), 2) - pow(self.xPos - i[0], 2) - pow(self.yPos - i[1], 2) + pow(i[2], 2)
		
				#only calculates distance if we know it isn't complex
				if (descriminant > 0):
					t = (cos(netAngle)*(i[0] - self.xPos) + sin(netAngle)*(i[1] - self.yPos) - pow(descriminant, 0.5))/100
					#if t is within an acceptable range, it gets returned
					if((t > 1) | (t < 0)):
						t = 1
				else:
					t = 1
	
				self.eyes[j] = max(self.eyes[j], 1-t)

			#this bit checks for intersection with the boundaries
			if(netAngle == 0):
				#if the eye is pointing straigt forward
				t = (bounds[3] - self.xPos)/100
			elif(netAngle == pi):
				#if the eye is poitning straight back
				t = (self.xPos - bounds[0])/100
			elif(netAngle == pi/2):
				#if the eye is pointing straight up
				t = (bounds[1] - self.yPos)/100
			elif(netAngle == 3*pi/2):
				#if the eye is pointing straight down
				t = (self.yPos - bounds[2])/100
			else:
				#any other angle
				t = ((bounds[0] - self.xPos)/cos(netAngle))/100
				if(t < 0):
					#t = ((bounds[3] - self.xPos)/cos(netAngle))/100
					t = 1
					#the eyes can't see the right side of the arena to prevent the agent from avoiding the finish line

				t2 = (bounds[1] - self.yPos)/(100*sin(netAngle))
				if(t2 < 0):
					t2 = (bounds[2] - self.yPos)/(100*sin(netAngle))

				t = min(t, t2)

			self.eyes[j] = max(self.eyes[j], 1-t)

	#'kills' the agent, sets its velocity and such to zero
	def die(self):
		#stops the agent from thinking, effectively killing it
		self.alive = 0

	#to be drawn, the main file needs to know the colour of the agent
	def getColour(self):
		return self.DNA[0]

	#in order to be reproduced, the agent needs to be able pass its DNA on, this function does just that
	def getDNA(self):
		return self.DNA

	#returns pointList so that functions outside this class can draw the agent
	def getPointList(self):
		return self.pointList

	#since each agent will be evaluated based on how far into the arena they make it, the x position can be used as a metric for performance
	def getX(self):
		return self.xPos

	#this function checks all the neurons and connections between them to see if they're above the limits
	#for example, connections between neurons cannot exceed an absolute value of two
	def limit(self, inDNA):
		#limits the spread of the eyes
		if(inDNA[1] > pi/3):
			inDNA[1] = pi/3
		elif(inDNA[1] < pi/12):
			inDNA[1] = pi/12

		#limits the default charge of all the neurons to between -1 and 1
		for i in range(0,len(inDNA[2])):
			for j in range(0,len(inDNA[2][i])):
				if(inDNA[2][i][j] > 1):
					inDNA[2][i][j] = 1
				elif(inDNA[2][i][j] < -1):
					inDNA[2][i][j] = -1

		#limits the connection strength between neurons to between -2 and 2
		for i in range(0,len(inDNA[3])):
			for j in range(0,len(inDNA[3][i])):
				for k in range(0,len(inDNA[3][i][j])):
					if(inDNA[3][i][j][k] > 2):
						inDNA[3][i][j][k]  = 2
					elif(inDNA[3][i][j][k]  < -2):
						inDNA[3][i][j][k]  = -2 

		return inDNA

	#revives the agent, and clears all of its neurons
	def revive(self):
		#revives the agent
		self.alive = 1
		#clears it brain of the last test run
		for i in self.brain:
			for j in i:
				j.clear()

	def setPosition(self, x, y):
		self.xPos = x
		self.yPos = y

	#this is where the AI stuff happens, this function takes all the inputs from the eyes and turns it into movement
	#this only gets called when the boolean alive is true
	def think(self):

		#gets the input from the eyes and passes it into the first layer of neurons
		for i in range(0,3):
			for j in range(0,3):
				self.brain[0][i].input(self.meshOne[i][j]*self.eyes[j])

		#passes info from the first layer into the second layer
		mesh = []
		for i in range(0,3):
			mesh = self.brain[0][i].getMesh()
			for j in range(0,3):
				self.brain[1][j].input(mesh[j]*self.brain[0][i].output())

		#passes info from the second layer to the output thrusters which control the agent
		for i in range(0,3):
			#pass
			self.turn(-self.brain[1][i].output()*self.brain[1][i].getMesh()[0])
			self.setVel(self.brain[1][i].output()*self.brain[1][i].getMesh()[1])
			self.turn(self.brain[1][i].output()*self.brain[1][i].getMesh()[2])

		#resets each neuron to its default state
		for i in self.brain:
			for j in i:
				j.clear()



	#updates the position and direction of the agent
	def update(self):
		#this is the control step, this is when the agent's AI alter the velocity, or direction to control it
		if(self.alive):
			self.think()

		if(self.xPos >= 1600):
			self.xPos += 1
		
		#the x and y position of the tip of the triangle, updated with velocity
		self.xPos += self.vel*cos(self.theta)
		self.yPos += self.vel*sin(self.theta)
		#sets velocity to zero so the agent will stop when the button isn't being pressed
		self.vel = 0

		#sets up the triabgular body of the agent
		self.pointList = [(self.xPos, self.yPos),
		 (self.xPos + 20*cos(7*pi/8 + self.theta), self.yPos + 20*sin(7*pi/8 + self.theta)),
		 (self.xPos + 20*cos(9*pi/8 + self.theta), self.yPos + 20*sin(9*pi/8 + self.theta))]


	#these are control functions, they allow the AI and other stuff to alter the movement of the agent
	def setVel(self, x):
		self.vel += 10*x

	def turn(self, x):
		self.theta += x/10

