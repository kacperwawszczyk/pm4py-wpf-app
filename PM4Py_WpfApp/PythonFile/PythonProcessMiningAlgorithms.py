import sys
import random
import string
import pm4py
from pm4py.objects.log.importer.xes import importer as xes_importer
from pm4py.algo.discovery.alpha import algorithm as alpha_algorithm
from pm4py.algo.discovery.inductive import algorithm as inductive_miner
from pm4py.visualization.petri_net import visualizer as pn_visualizer
from pm4py.visualization.heuristics_net import visualizer as hn_visualizer
from pm4py.visualization.process_tree import visualizer as pt_visualizer

log = xes_importer.apply(sys.argv[2])

# Argument 1 - nazwa funkcji jaka chcemy wywolac w wierszu polecen
# Argument 2 - plik XES
# Argument 3 - folder, w jakim maja byc zapisywane obrazy


# Tworzy obraz PNG dla odpowiedniego modelu procesu wedlug wyrazenia regularnego "tmp[a-zA-Z0-9]{10}\.png"
# model - "pn" dla sieci Petriego, "hn" dla sieci heurystycznej, "pt" dla drzewa procesu
# net - model procesu
# im - marking poczatkowy (tylko dla sieci Petriego)
# fm - marking koncowy (tylko dla sieci Petriego)
def generate_image(model, net, im=None, fm=None):
    filename = "tmp" + ''.join(random.choice(string.ascii_letters + string.digits) for i in range(10)) + ".png"
    if model == "pn":
        gviz = pn_visualizer.apply(net, im, fm)
        pn_visualizer.save(gviz, sys.argv[3] + filename)
    elif model == "hn":
        gviz = hn_visualizer.apply(net)
        hn_visualizer.save(gviz, sys.argv[3] + filename)
    elif model == "pt":
        gviz = pt_visualizer.apply(net)
        pt_visualizer.save(gviz, sys.argv[3] + filename)
    print(filename)


# Odkrywa siec Petriego przy uzyciu algorytmu Alpha Miner lub Alpha+ Miner
# Argument 4 - "classic" dla algorytmu alpha, "plus" dla algorytmu alpha+
# Argument 5 - "remove_unconnected" gdy chcemy usunac niepolaczone tranzycje, "leave_unconnected" gdy chcemy je zostawic (tylko dla algorytmu alpha+)
def alpha_miner():
    parameters = {}
    if sys.argv[4] == "classic":
        variant = alpha_algorithm.Variants.ALPHA_VERSION_CLASSIC
    elif sys.argv[4] == "plus":
        variant = alpha_algorithm.Variants.ALPHA_VERSION_PLUS
        if sys.argv[5] == "remove_unconnected":
            parameters = {alpha_algorithm.Variants.ALPHA_VERSION_PLUS.value.Parameters.REMOVE_UNCONNECTED: True}   
        elif sys.argv[5] == "leave_unconnected":
            parameters = {alpha_algorithm.Variants.ALPHA_VERSION_PLUS.value.Parameters.REMOVE_UNCONNECTED: False} 
    net, initial_marking, final_marking = alpha_algorithm.apply(log, variant=variant, parameters=parameters)
    generate_image("pn", net, initial_marking, final_marking)

# Odkrywa siec Petriego przy uzyciu algorytmu Heuristics Miner
# Argument 4 - wartosc progu zaleznosci
# Argument 5 - wartosc progu miary AND
# Argument 6 - wartosc progu petli o dlugosci dwa
def heuristics_miner_petri_net():
    net, initial_marking, final_marking = pm4py.discover_petri_net_heuristics(log, dependency_threshold=float(sys.argv[4]), and_threshold=float(sys.argv[5]), loop_two_threshold=float(sys.argv[6]))
    generate_image("pn", net, initial_marking, final_marking)

# Odkrywa siec heurystyczna przy uzyciu algorytmu Heuristics Miner
# Argument 4 - wartosc progu zaleznosci
# Argument 5 - wartosc progu miary AND
# Argument 6 - wartosc progu petli o dlugosci dwa
def heuristics_net():
    heu_net = pm4py.discover_heuristics_net(log, dependency_threshold=float(sys.argv[4]), and_threshold=float(sys.argv[5]), loop_two_threshold=float(sys.argv[6]))
    generate_image("hn", heu_net)

# Odkrywa siec Petriego przy uzyciu algorytmu Inductive Miner
# Argument 4 - "IM" dla podstawowego wariantu Inductive Minera, "IMf" dla Inductive Miner infrequent, "IMd" dla Inductive Miner directly-follows
# Argument 5 - wartosc progu szumu (tylko dla Inductive Miner infrequent)
def inductive_miner_petri_net():
    if sys.argv[4] == "IM":
        variant = inductive_miner.Variants.IM
    elif sys.argv[4] == "IMf":
        variant = inductive_miner.Variants.IMf
    elif sys.argv[4] == "IMd":
        variant = inductive_miner.Variants.IMd
    parameters = {inductive_miner.Variants.IM.value.Parameters.NOISE_THRESHOLD: float(sys.argv[5])}
    net, initial_marking, final_marking = inductive_miner.apply(log, variant=variant, parameters=parameters)
    generate_image("pn", net, initial_marking, final_marking)

# Odkrywa drzewo procesu przy uzyciu algorytmu Inductive Miner
# Argument 4 - wartosc progu szumu (tylko dla Inductive Miner infrequent)
def process_tree():
    tree = pm4py.discover_process_tree_inductive(log, noise_threshold=float(sys.argv[4]))
    generate_image("pt", tree)


if __name__ == '__main__':
    # Pozwala wczytac nazwe funkcji jako pierwszy argument w wierszu polecen
    globals()[sys.argv[1]]()