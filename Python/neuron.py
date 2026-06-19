class neuron:
	
	def __init__(self, x, mesh):
		#This will take and store a value every cycle, as well as output that value 
		#when called by an external class. This is a slave class to agent, it is part of a meshnet of other neurons which make up
		#the AI of that agent. The meshnet will multiply the output by a certain constant before inputting it into another neuron.
		#the behavior of this class was based off the behavior of actual neurons in a brain

		#this is the data stored by the neuron
		self.charge = x

		#this is the base charge of the neuron
		self.baseCharge = x

		#this is the array of inputs the neuron passes on to the next layer
		self.mesh = mesh

	def clear(self):
		self.charge = self.baseCharge

	def getMesh(self):
		return self.mesh

	def input(self, x):
		self.charge += x

	def output(self):
		return self.charge

