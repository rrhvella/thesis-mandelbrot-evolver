This repository contains the code I developed as part of my undergraduate thesis. The scope of the project was to experiment with the use of complex neuroevolution -- the evolution of neural networks with complex-valued weights and activations -- to explore the space of Mandelbrot sets. This was accomplished using the CPPN-NEAT algorithm [17], and the ideas on Mandelbrot evolution discussed by Ashlock [1] [5].

The main user interface used to generate the artefacts can be accessed by building and running the 'ComplexCPPNNEATSelection' project in Visual Studio 2010 or greater.

The following screenshot shows how the user interface is laid out.

![User interface areas](https://raw.github.com/rrhvella/thesis-mandelbrot-evolver/master/ui.png)

And the following list describes the outcomes of each action which can be performed on the interface.

Action	|	Outcomes
---	|	---
Left-clicking on a thumbnail in the ‘fractal selection panel’.	|	The score of the genome represented by that thumbnail is increased by 1.
Right-clicking on a thumbnail in the ‘fractal selection panel’.	|	The source genome of ‘fractal image preview’ is replaced with the genome represented by that thumbnail. See text below for more info.
Hovering on a thumbnail in the ‘fractal selection panel’.	|	Replaces the contents of ‘genome info’ with the textual representation of the genome represented by that thumbnail. 
Pressing the ‘Enter’ key.	|	* Iterates the genetic algorithm and replaces the thumbnails in the ‘fractal selection panel’ with the ones for the new generation.<br>* Increases the number in the ‘generations label’ by 1.<br>* Resets the scores of all the genomes.
Clicking the output button.	|	Saves the genome displayed in the ‘fractal image preview’ to disk. 

As this project was developed based on ideas discussed in published works, a list of references can be found in the [REFERENCES file](https://github.com/rrhvella/thesis-mandelbrot-evolver/blob/master/REFERENCES).
